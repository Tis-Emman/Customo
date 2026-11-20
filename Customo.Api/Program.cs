using Customo.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomoDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddSignalR();

// Let the Next.js app (port 3000) call this API and connect to the SignalR hub
// Allowed website addresses. Locally it is localhost:3000; in the cloud it comes from the Cors__Origins__0 setting.
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(o => o.AddPolicy("web", p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
// A table someone picked but never ordered from is given back after this long.
// Once the party has ordered, the table stays occupied until staff free it on the kitchen screen.
var tableHold = TimeSpan.FromHours(3);
app.UseCors("web");
app.MapHub<OrdersHub>("/hubs/orders");

// ---------- Menu ----------

// GET /api/menu: all dishes with their ingredients, allergens, options, and what is sold out
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
            .Select(x => new IngredientDto(x.IngredientId, x.Ingredient.Name, x.Ingredient.Allergen?.Name, x.IsRemovable,
                Stock.Has(x.Ingredient, x.Quantity))).ToList(),
        m.Options.OrderBy(o => o.Id)
            .Select(o => new OptionDto(o.Id, o.Type.ToString(), o.Name, o.PriceDelta)).ToList(),
        // A dish is sold out when an ingredient it cannot be made without has run out
        m.Ingredients.Where(x => !x.IsRemovable).All(x => Stock.Has(x.Ingredient, x.Quantity))));
});

// ---------- Orders ----------

// POST /api/orders: validate, recompute prices, check stock, flag allergy conflicts, save
app.MapPost("/api/orders", async (NewOrderRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    var takeOut = string.Equals(req.Type, "TakeOut", StringComparison.OrdinalIgnoreCase);
    if (req.Items is null || req.Items.Count == 0) return Results.BadRequest("The order has no items.");

    // Dine-in: only the customer who holds the table may order for it
    if (!takeOut)
    {
        if (req.Table is not (>= 1 and <= 99)) return Results.BadRequest("Invalid table number.");
        var cutoff = DateTime.UtcNow - tableHold;
        var held = await db.Tables.AnyAsync(t => t.Number == req.Table && req.TableToken != null &&
            t.ClaimToken == req.TableToken && (t.HasOrdered || t.OccupiedAt >= cutoff));
        if (!held) return Results.Conflict("Your table is no longer reserved. Please pick it again.");
    }

    var names = req.Allergies ?? new();
    var allergies = await db.Allergens.Where(a => names.Contains(a.Name)).ToListAsync();
    var allergyNames = allergies.Select(a => a.Name).ToHashSet();

    var ids = req.Items.Select(i => i.MenuItemId).Distinct().ToList();
    var menu = await db.MenuItems.AsSplitQuery()
        .Include(m => m.Options)
        .Include(m => m.Ingredients).ThenInclude(x => x.Ingredient).ThenInclude(i => i.Allergen)
        .Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

    var order = new Order
    {
        Type = takeOut ? OrderType.TakeOut : OrderType.DineIn,
        TableNumber = takeOut ? null : req.Table,
        Allergies = allergies
    };
    var needed = new Dictionary<int, (Ingredient Ing, decimal Amount, HashSet<string> Dishes)>();

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
        var used = dish.Ingredients.Where(x => !removed.Contains(x)).ToList();

        // Sold out: the kitchen marked it, or tracked stock has run out
        foreach (var x in used.Where(x => !Stock.Has(x.Ingredient, x.Quantity)))
            return Results.Conflict(x.IsRemovable
                ? $"{x.Ingredient.Name} for {dish.Name} is sold out. Please remove it from the dish."
                : $"Sorry, {dish.Name} is sold out. Please remove it from your order.");

        // Add up how much of each tracked ingredient the whole order uses
        foreach (var x in used.Where(x => x.Ingredient.Stock is not null))
        {
            var cur = needed.TryGetValue(x.IngredientId, out var n) ? n : (x.Ingredient, 0m, new HashSet<string>());
            cur.Item3.Add(dish.Name);
            needed[x.IngredientId] = (cur.Item1, cur.Item2 + x.Quantity * line.Quantity, cur.Item3);
        }

        // Conflict = an allergen the customer declared is still in the dish after the removals
        var conflict = used.Any(x => x.Ingredient.Allergen is not null && allergyNames.Contains(x.Ingredient.Allergen.Name));

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

    // Quick check before touching the database, so the message can name the dishes
    foreach (var (ing, amount, dishes) in needed.Values.Where(n => n.Ing.Stock < n.Amount))
        return Results.Conflict($"Not enough {ing.Name.ToLower()} left for this order ({ing.Stock:0.##} {ing.Unit}). Please order fewer {string.Join(", ", dishes)}.");

    order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

    // Save in one transaction: take the stock (atomic UPDATEs, so two tablets can't sell the last portion twice),
    // mark the table as ordered, and save the order. If anything fails, nothing is changed.
    var strategy = db.Database.CreateExecutionStrategy();
    var problem = await strategy.ExecuteAsync(async () =>
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        foreach (var (id, (ing, amount, _)) in needed)
        {
            var ok = await db.Ingredients
                .Where(i => i.Id == id && i.IsAvailable && i.Stock != null && i.Stock >= amount)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Stock, i => i.Stock - amount));
            if (ok == 0) { await tx.RollbackAsync(); return $"Sorry, we just ran out of {ing.Name.ToLower()}. Please update your order."; }
        }
        if (!takeOut)
        {
            var ok = await db.Tables
                .Where(t => t.Number == req.Table && t.ClaimToken == req.TableToken)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.HasOrdered, true).SetProperty(t => t.OccupiedAt, DateTime.UtcNow));
            if (ok == 0) { await tx.RollbackAsync(); return "Your table is no longer reserved. Please pick it again."; }
        }
        if (!db.Orders.Local.Contains(order)) db.Orders.Add(order);
        foreach (var (id, (_, amount, _)) in needed)
            if (!db.StockMovements.Local.Any(m => m.IngredientId == id && m.Order == order))
                db.StockMovements.Add(new StockMovement { IngredientId = id, Change = -amount, Reason = StockReason.Sale, Order = order });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return null;
    });
    if (problem is not null) return Results.Conflict(problem);

    // Tell every connected kitchen screen about the new order
    var dto = OrderMapper.ToDto(order);
    await hub.Clients.All.SendAsync("OrderSubmitted", dto);
    if (!takeOut) await hub.Clients.All.SendAsync("TablesChanged");
    if (needed.Count > 0) await hub.Clients.All.SendAsync("MenuChanged");   // stock went down
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

// ---------- Tables ----------

// GET /api/tables?token=...: which tables are taken ("mine" = held by the caller's browser)
app.MapGet("/api/tables", async (CustomoDbContext db, string? token) =>
{
    var cutoff = DateTime.UtcNow - tableHold;
    var rows = await db.Tables.AsNoTracking().OrderBy(t => t.Number).ToListAsync();
    return rows.Select(t =>
    {
        var live = t.ClaimToken != null && (t.HasOrdered || t.OccupiedAt >= cutoff);
        var mine = live && token != null && t.ClaimToken == token;
        return new TableDto(t.Number, live && !mine, mine, live && t.HasOrdered);
    });
});

// POST /api/tables/{n}/claim: one atomic UPDATE, so two tablets can never win the same table
app.MapPost("/api/tables/{number:int}/claim", async (int number, ClaimRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    if (!await db.Tables.AnyAsync(t => t.Number == number)) return Results.NotFound();

    var cutoff = DateTime.UtcNow - tableHold;
    var token = Guid.NewGuid().ToString("N");
    var mine = req.Token;
    var rows = await db.Tables
        .Where(t => t.Number == number &&
                    (t.ClaimToken == null || (!t.HasOrdered && t.OccupiedAt < cutoff) || (mine != null && t.ClaimToken == mine)))
        .ExecuteUpdateAsync(s => s
            .SetProperty(t => t.HasOrdered, t => mine != null && t.ClaimToken == mine && t.HasOrdered)   // same party keeps its status
            .SetProperty(t => t.ClaimToken, token)
            .SetProperty(t => t.OccupiedAt, DateTime.UtcNow));

    if (rows == 0) return Results.Conflict("That table is taken.");
    await hub.Clients.All.SendAsync("TablesChanged");
    return Results.Ok(new { token });
});

// POST /api/tables/{n}/leave: a customer gives back a table they picked by mistake.
// Only works with their own token, and never after they ordered, so a shared tablet can't free a seated party.
app.MapPost("/api/tables/{number:int}/leave", async (int number, ClaimRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    if (req.Token is null) return Results.BadRequest();
    var rows = await db.Tables.Where(t => t.Number == number && t.ClaimToken == req.Token && !t.HasOrdered)
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.ClaimToken, (string?)null).SetProperty(t => t.OccupiedAt, (DateTime?)null));
    if (rows > 0) await hub.Clients.All.SendAsync("TablesChanged");
    return Results.NoContent();
});

// POST /api/tables/{n}/release: staff free a table (no login in this version, like the rest of the kitchen screen)
app.MapPost("/api/tables/{number:int}/release", async (int number, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    await db.Tables.Where(t => t.Number == number)
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.ClaimToken, (string?)null)
            .SetProperty(t => t.OccupiedAt, (DateTime?)null).SetProperty(t => t.HasOrdered, false));
    await hub.Clients.All.SendAsync("TablesChanged");
    return Results.NoContent();
});

// ---------- Inventory (staff) ----------

// GET /api/inventory: every ingredient with its stock and which dishes use it
app.MapGet("/api/inventory", async (CustomoDbContext db) =>
{
    var ings = await db.Ingredients.AsNoTracking().Include(i => i.Allergen).OrderBy(i => i.Name).ToListAsync();
    var uses = await db.MenuItemIngredients.AsNoTracking()
        .Select(x => new { x.IngredientId, x.MenuItem.Name }).ToListAsync();
    var byIng = uses.GroupBy(u => u.IngredientId).ToDictionary(g => g.Key, g => g.Select(u => u.Name).OrderBy(n => n).ToList());

    return ings.Select(i => new InventoryDto(i.Id, i.Name, i.Unit, i.Stock, i.LowAt, i.IsAvailable, i.Allergen?.Name,
        byIng.GetValueOrDefault(i.Id) ?? new()));
});

// PUT /api/inventory/{id}: settings (unit, low-stock alert, available / sold out)
app.MapPut("/api/inventory/{id:int}", async (int id, InventorySettings req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    var unit = req.Unit?.Trim();
    if (string.IsNullOrEmpty(unit) || unit.Length > 20) return Results.BadRequest("Enter a unit (up to 20 letters).");
    if (req.LowAt is < 0) return Results.BadRequest("The low-stock alert can't be negative.");

    var rows = await db.Ingredients.Where(i => i.Id == id).ExecuteUpdateAsync(s => s
        .SetProperty(i => i.Unit, unit).SetProperty(i => i.LowAt, req.LowAt).SetProperty(i => i.IsAvailable, req.IsAvailable));
    if (rows == 0) return Results.NotFound();
    await hub.Clients.All.SendAsync("MenuChanged");
    return Results.NoContent();
});

// POST /api/inventory/{id}/stock: a delivery came in (restock), something was thrown away (waste),
// staff counted what is left (count), or the item should no longer be counted (untrack)
app.MapPost("/api/inventory/{id:int}/stock", async (int id, StockRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    var ing = await db.Ingredients.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    if (ing is null) return Results.NotFound();
    var amount = req.Amount;
    var kind = req.Kind?.ToLowerInvariant();
    if (kind != "untrack" && (amount < 0 || amount > 100000)) return Results.BadRequest("Enter an amount from 0 to 100000.");

    StockMovement? move = null;
    switch (kind)
    {
        case "restock":
            if (amount == 0) return Results.BadRequest("Enter how much came in.");
            await db.Ingredients.Where(i => i.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Stock, i => (i.Stock ?? 0) + amount));
            move = new() { IngredientId = id, Change = amount, Reason = StockReason.Restock, Note = req.Note };
            break;
        case "waste":
            if (amount == 0) return Results.BadRequest("Enter how much was thrown away.");
            if (ing.Stock is null) return Results.BadRequest("Count this item first, then record waste.");
            await db.Ingredients.Where(i => i.Id == id && i.Stock != null)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Stock, i => i.Stock < amount ? 0 : i.Stock - amount));
            move = new() { IngredientId = id, Change = -Math.Min(amount, ing.Stock.Value), Reason = StockReason.Waste, Note = req.Note };
            break;
        case "count":
            await db.Ingredients.Where(i => i.Id == id).ExecuteUpdateAsync(s => s.SetProperty(i => i.Stock, (decimal?)amount));
            move = new() { IngredientId = id, Change = amount - (ing.Stock ?? 0), Reason = StockReason.Count, Note = req.Note };
            break;
        case "untrack":
            await db.Ingredients.Where(i => i.Id == id).ExecuteUpdateAsync(s => s.SetProperty(i => i.Stock, (decimal?)null));
            break;
        default:
            return Results.BadRequest("Unknown stock action.");
    }
    if (move is not null) { db.StockMovements.Add(move); await db.SaveChangesAsync(); }
    await hub.Clients.All.SendAsync("MenuChanged");
    return Results.NoContent();
});

// GET /api/inventory/movements: the latest stock changes, newest first
app.MapGet("/api/inventory/movements", async (CustomoDbContext db, int? take) =>
{
    var n = Math.Clamp(take ?? 40, 1, 200);
    var rows = await db.StockMovements.AsNoTracking().OrderByDescending(m => m.Id).Take(n)
        .Select(m => new { m.Id, m.Ingredient.Name, m.Ingredient.Unit, m.Change, m.Reason, m.OrderId, m.Note, m.CreatedAt })
        .ToListAsync();
    return rows.Select(m => new MovementDto(m.Id, m.Name, m.Unit, m.Change, m.Reason.ToString(), m.OrderId, m.Note,
        new DateTimeOffset(DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds()));
});

app.Run();

// SignalR hub: screens connect here and listen for OrderSubmitted, OrderCompleted, TablesChanged, MenuChanged
class OrdersHub : Hub { }

static class Stock
{
    // Enough of this ingredient for one serving? (not marked sold out, and untracked or still in stock)
    public static bool Has(Ingredient i, decimal perServing) => i.IsAvailable && (i.Stock is null || i.Stock >= perServing);
}

// ---------- Request and response shapes ----------
record OptionDto(int Id, string Type, string Name, decimal Delta);
record IngredientDto(int Id, string Name, string? Allergen, bool Removable, bool Available);
record MenuItemDto(int Id, string Name, string Cat, decimal Price, List<string> Allergens, List<IngredientDto> Ingredients, List<OptionDto> Options, bool Available);

record NewOrderLine(int MenuItemId, int Quantity, List<int>? OptionIds, List<int>? RemovedIngredientIds);
record NewOrderRequest(string? Type, int? Table, string? TableToken, List<string>? Allergies, List<NewOrderLine>? Items);
record TableDto(int Number, bool Taken, bool Mine, bool Ordered);
record ClaimRequest(string? Token);

record OrderItemDto(string Name, int Qty, List<string> Opts, List<string> Removed, bool Conflict);
record OrderDto(int Id, string Type, int? Table, List<string> Allergies, List<OrderItemDto> Items, decimal Total, long CreatedAt);

record InventoryDto(int Id, string Name, string Unit, decimal? Stock, decimal? LowAt, bool Available, string? Allergen, List<string> UsedIn);
record InventorySettings(string? Unit, decimal? LowAt, bool IsAvailable);
record StockRequest(string? Kind, decimal Amount, string? Note);
record MovementDto(int Id, string Ingredient, string Unit, decimal Change, string Reason, int? OrderId, string? Note, long At);

static class OrderMapper
{
    // Opts = size, extras, and swaps that differ from the default. Removed = ingredients left out.
    public static OrderDto ToDto(Order o) => new(
        o.Id, o.Type.ToString(), o.TableNumber,
        o.Allergies.Select(a => a.Name).ToList(),
        o.Items.Select(i => new OrderItemDto(
            i.MenuItem.Name, i.Quantity,
            i.Options.Where(x => x.Type != OptionType.Remove).Select(x => x.OptionName).ToList(),
            i.Options.Where(x => x.Type == OptionType.Remove).Select(x => x.OptionName).ToList(),
            i.HasAllergyConflict)).ToList(),
        o.TotalAmount,
        new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds());
}
