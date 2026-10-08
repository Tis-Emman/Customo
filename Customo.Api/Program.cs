using Customo.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomoDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSignalR();

// Let the Next.js app (port 3000) call this API and connect to the SignalR hub
// (SignalR needs AllowCredentials, which only works with a specific origin)
builder.Services.AddCors(o => o.AddPolicy("web", p =>
    p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
var tableHold = TimeSpan.FromHours(3);   // a table stays held this long after its last activity
app.UseCors("web");
app.MapHub<OrdersHub>("/hubs/orders");

// GET /api/menu: all dishes with their ingredients, allergens, and options
app.MapGet("/api/menu", async (CustomoDbContext db) =>
{
    var items = await db.MenuItems.AsNoTracking().AsSplitQuery()
        .Include(m => m.Category).Include(m => m.Options)
        .Include(m => m.Ingredients).ThenInclude(x => x.Ingredient).ThenInclude(i => i.Allergen)
        .OrderBy(m => m.Id).ToListAsync();

    return items.Select(m => new MenuItemDto(
        m.Id, m.Name, m.Category.Name, m.Price,
        m.Ingredients.Select(x => x.Ingredient.Allergen?.Name).OfType<string>().Distinct().ToList(),
        m.Ingredients.OrderBy(x => x.IsRemovable).ThenBy(x => x.IngredientId)
            .Select(x => new IngredientDto(x.IngredientId, x.Ingredient.Name, x.Ingredient.Allergen?.Name, x.IsRemovable)).ToList(),
        m.Options.OrderBy(o => o.Id)
            .Select(o => new OptionDto(o.Id, o.Type.ToString(), o.Name, o.PriceDelta)).ToList()));
});

// POST /api/orders: validate, recompute prices on the server, flag allergy conflicts, save
app.MapPost("/api/orders", async (NewOrderRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    if (req.Table < 1 || req.Table > 99) return Results.BadRequest("Invalid table number.");
    if (req.Items is null || req.Items.Count == 0) return Results.BadRequest("The order has no items.");

    // Only the customer who holds the table may order for it
    var table = await db.Tables.FirstOrDefaultAsync(t => t.Number == req.Table);
    if (table is null) return Results.BadRequest("Invalid table number.");
    if (req.TableToken is null || table.ClaimToken != req.TableToken || table.OccupiedAt < DateTime.UtcNow - tableHold)
        return Results.Conflict("Your table is no longer reserved. Please pick it again.");

    var names = req.Allergies ?? new();
    var allergies = await db.Allergens.Where(a => names.Contains(a.Name)).ToListAsync();
    var allergyNames = allergies.Select(a => a.Name).ToHashSet();

    var ids = req.Items.Select(i => i.MenuItemId).Distinct().ToList();
    var menu = await db.MenuItems.AsSplitQuery()
        .Include(m => m.Options)
        .Include(m => m.Ingredients).ThenInclude(x => x.Ingredient).ThenInclude(i => i.Allergen)
        .Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

    var order = new Order { TableNumber = req.Table, Allergies = allergies };

    foreach (var line in req.Items)
    {
        if (line.Quantity < 1 || line.Quantity > 20) return Results.BadRequest("Invalid quantity.");
        if (!menu.TryGetValue(line.MenuItemId, out var dish))
            return Results.BadRequest($"Menu item {line.MenuItemId} was not found.");

        // Options (size, extras, swaps)
        var picked = new List<MenuItemOption>();
        foreach (var optionId in line.OptionIds ?? new())
        {
            var opt = dish.Options.FirstOrDefault(o => o.Id == optionId);
            if (opt is null) return Results.BadRequest($"Option {optionId} does not belong to {dish.Name}.");
            picked.Add(opt);
        }
        // Size and Swap are single-choice; Extras can be many
        if (picked.Where(o => o.Type != OptionType.Extra).GroupBy(o => o.Type).Any(g => g.Count() > 1))
            return Results.BadRequest($"Pick only one size or swap for {dish.Name}.");

        // Ingredients the customer left out: must belong to the dish and be removable
        var removed = new List<MenuItemIngredient>();
        foreach (var ingredientId in line.RemovedIngredientIds ?? new())
        {
            var link = dish.Ingredients.FirstOrDefault(x => x.IngredientId == ingredientId);
            if (link is null) return Results.BadRequest($"Ingredient {ingredientId} is not part of {dish.Name}.");
            if (!link.IsRemovable) return Results.BadRequest($"{link.Ingredient.Name} cannot be removed from {dish.Name}.");
            if (!removed.Contains(link)) removed.Add(link);
        }

        // Conflict = an allergen the customer declared is still in the dish after the removals
        var conflict = dish.Ingredients.Where(x => !removed.Contains(x))
            .Any(x => x.Ingredient.Allergen is not null && allergyNames.Contains(x.Ingredient.Allergen.Name));

        // Skip the default size/swap (first of its type, no extra cost): the kitchen only needs the changes
        var defaults = dish.Options.Where(o => o.Type != OptionType.Extra).GroupBy(o => o.Type)
            .Select(g => g.OrderBy(o => o.Id).First().Id).ToHashSet();
        var shown = picked.Where(o => !(defaults.Contains(o.Id) && o.PriceDelta == 0));

        var saved = shown.Select(o => new OrderItemOption { Type = o.Type, OptionName = o.Name, PriceDelta = o.PriceDelta }).ToList();
        saved.AddRange(removed.Select(r => new OrderItemOption { Type = OptionType.Remove, OptionName = r.Ingredient.Name, PriceDelta = 0 }));

        order.Items.Add(new OrderItem
        {
            MenuItem = dish,
            Quantity = line.Quantity,
            UnitPrice = dish.Price + picked.Sum(o => o.PriceDelta),
            HasAllergyConflict = conflict,
            Options = saved
        });
    }

    order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
    table.OccupiedAt = DateTime.UtcNow;   // ordering restarts the 3-hour hold
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    // Tell every connected kitchen screen about the new order
    var dto = OrderMapper.ToDto(order);
    await hub.Clients.All.SendAsync("OrderSubmitted", dto);
    return Results.Ok(dto);
});

// GET /api/orders: active (not completed) orders from the last 12 hours, oldest first (for the kitchen screen)
app.MapGet("/api/orders", async (CustomoDbContext db) =>
{
    var since = DateTime.UtcNow.AddHours(-12);
    var orders = await db.Orders.AsNoTracking().AsSplitQuery()
        .Include(o => o.Allergies)
        .Include(o => o.Items).ThenInclude(i => i.MenuItem)
        .Include(o => o.Items).ThenInclude(i => i.Options)
        .Where(o => o.CompletedAt == null && o.CreatedAt >= since)
        .OrderBy(o => o.CreatedAt).ToListAsync();

    return orders.Select(OrderMapper.ToDto);
});

// POST /api/orders/{id}/complete: the chef is done, so the order leaves every kitchen screen.
// The order is kept in the database (soft delete), so history is not lost.
app.MapPost("/api/orders/{id:int}/complete", async (int id, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();

    if (order.CompletedAt is null)
    {
        order.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
    await hub.Clients.All.SendAsync("OrderCompleted", id);
    return Results.NoContent();
});

// GET /api/tables?token=...: which tables are taken ("mine" = held by the caller's browser)
app.MapGet("/api/tables", async (CustomoDbContext db, string? token) =>
{
    var cutoff = DateTime.UtcNow - tableHold;
    var rows = await db.Tables.AsNoTracking().OrderBy(t => t.Number).ToListAsync();
    return rows.Select(t =>
    {
        var live = t.ClaimToken != null && t.OccupiedAt >= cutoff;
        var mine = live && token != null && t.ClaimToken == token;
        return new TableDto(t.Number, live && !mine, mine);
    });
});

// POST /api/tables/{n}/claim: one atomic UPDATE, so two tablets can never win the same table
app.MapPost("/api/tables/{number:int}/claim", async (int number, ClaimRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    if (!await db.Tables.AnyAsync(t => t.Number == number)) return Results.NotFound();

    var cutoff = DateTime.UtcNow - tableHold;
    var token = Guid.NewGuid().ToString("N");
    var rows = await db.Tables
        .Where(t => t.Number == number &&
                    (t.ClaimToken == null || t.OccupiedAt < cutoff || (req.Token != null && t.ClaimToken == req.Token)))
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.ClaimToken, token).SetProperty(t => t.OccupiedAt, DateTime.UtcNow));

    if (rows == 0) return Results.Conflict("That table is taken.");
    await hub.Clients.All.SendAsync("TablesChanged");
    return Results.Ok(new { token });
});

// POST /api/tables/{n}/release: staff free a table (no login in this version, like the rest of the kitchen screen)
app.MapPost("/api/tables/{number:int}/release", async (int number, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    await db.Tables.Where(t => t.Number == number)
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.ClaimToken, (string?)null).SetProperty(t => t.OccupiedAt, (DateTime?)null));
    await hub.Clients.All.SendAsync("TablesChanged");
    return Results.NoContent();
});

app.Run();

// SignalR hub: the kitchen screen connects here and listens for "OrderSubmitted"
class OrdersHub : Hub { }

// ---------- Request and response shapes ----------
record OptionDto(int Id, string Type, string Name, decimal Delta);
record IngredientDto(int Id, string Name, string? Allergen, bool Removable);
record MenuItemDto(int Id, string Name, string Cat, decimal Price, List<string> Allergens, List<IngredientDto> Ingredients, List<OptionDto> Options);

record NewOrderLine(int MenuItemId, int Quantity, List<int>? OptionIds, List<int>? RemovedIngredientIds);
record NewOrderRequest(int Table, string? TableToken, List<string>? Allergies, List<NewOrderLine>? Items);
record TableDto(int Number, bool Taken, bool Mine);
record ClaimRequest(string? Token);

record OrderItemDto(string Name, int Qty, List<string> Opts, List<string> Removed, bool Conflict);
record OrderDto(int Id, int Table, List<string> Allergies, List<OrderItemDto> Items, decimal Total, long CreatedAt);

static class OrderMapper
{
    // Opts = size, extras, and swaps that differ from the default. Removed = ingredients left out.
    public static OrderDto ToDto(Order o) => new(
        o.Id, o.TableNumber,
        o.Allergies.Select(a => a.Name).ToList(),
        o.Items.Select(i => new OrderItemDto(
            i.MenuItem.Name, i.Quantity,
            i.Options.Where(x => x.Type != OptionType.Remove).Select(x => x.OptionName).ToList(),
            i.Options.Where(x => x.Type == OptionType.Remove).Select(x => x.OptionName).ToList(),
            i.HasAllergyConflict)).ToList(),
        o.TotalAmount,
        new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds());
}
