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
app.UseCors("web");
app.MapHub<OrdersHub>("/hubs/orders");

// GET /api/menu: all dishes with their allergens and options
app.MapGet("/api/menu", async (CustomoDbContext db) =>
{
    var items = await db.MenuItems.AsNoTracking().AsSplitQuery()
        .Include(m => m.Category).Include(m => m.Allergens).Include(m => m.Options)
        .OrderBy(m => m.Id).ToListAsync();

    return items.Select(m => new MenuItemDto(
        m.Id, m.Name, m.Category.Name, m.Price,
        m.Allergens.Select(a => a.Name).ToList(),
        m.Options.OrderBy(o => o.Id)
            .Select(o => new OptionDto(o.Id, o.Type.ToString(), o.Name, o.PriceDelta)).ToList()));
});

// POST /api/orders: validate, recompute prices on the server, flag allergy conflicts, save
app.MapPost("/api/orders", async (NewOrderRequest req, CustomoDbContext db, IHubContext<OrdersHub> hub) =>
{
    if (req.Table < 1 || req.Table > 99) return Results.BadRequest("Invalid table number.");
    if (req.Items is null || req.Items.Count == 0) return Results.BadRequest("The order has no items.");

    var names = req.Allergies ?? new();
    var allergies = await db.Allergens.Where(a => names.Contains(a.Name)).ToListAsync();
    var allergyNames = allergies.Select(a => a.Name).ToHashSet();

    var ids = req.Items.Select(i => i.MenuItemId).Distinct().ToList();
    var menu = await db.MenuItems.Include(m => m.Allergens).Include(m => m.Options)
        .Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

    var order = new Order { TableNumber = req.Table, Allergies = allergies };

    foreach (var line in req.Items)
    {
        if (line.Quantity < 1 || line.Quantity > 20) return Results.BadRequest("Invalid quantity.");
        if (!menu.TryGetValue(line.MenuItemId, out var dish))
            return Results.BadRequest($"Menu item {line.MenuItemId} was not found.");

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

        order.Items.Add(new OrderItem
        {
            MenuItem = dish,
            Quantity = line.Quantity,
            UnitPrice = dish.Price + picked.Sum(o => o.PriceDelta),
            HasAllergyConflict = dish.Allergens.Any(a => allergyNames.Contains(a.Name)),
            Options = picked.Select(o => new OrderItemOption { OptionName = o.Name, PriceDelta = o.PriceDelta }).ToList()
        });
    }

    order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    // Tell every connected kitchen screen about the new order
    var dto = OrderMapper.ToDto(order);
    await hub.Clients.All.SendAsync("OrderSubmitted", dto);
    return Results.Ok(dto);
});

// GET /api/orders: orders from the last 4 hours, oldest first (for the kitchen screen)
app.MapGet("/api/orders", async (CustomoDbContext db) =>
{
    var since = DateTime.UtcNow.AddHours(-4);
    var orders = await db.Orders.AsNoTracking().AsSplitQuery()
        .Include(o => o.Allergies)
        .Include(o => o.Items).ThenInclude(i => i.MenuItem)
        .Include(o => o.Items).ThenInclude(i => i.Options)
        .Where(o => o.CreatedAt >= since)
        .OrderBy(o => o.CreatedAt).ToListAsync();

    return orders.Select(OrderMapper.ToDto);
});

app.Run();

// SignalR hub: the kitchen screen connects here and listens for "OrderSubmitted"
class OrdersHub : Hub { }

// ---------- Request and response shapes ----------
record OptionDto(int Id, string Type, string Name, decimal Delta);
record MenuItemDto(int Id, string Name, string Cat, decimal Price, List<string> Allergens, List<OptionDto> Options);

record NewOrderLine(int MenuItemId, int Quantity, List<int>? OptionIds);
record NewOrderRequest(int Table, List<string>? Allergies, List<NewOrderLine>? Items);

record OrderItemDto(string Name, int Qty, List<string> Opts, bool Conflict);
record OrderDto(int Id, int Table, List<string> Allergies, List<OrderItemDto> Items, decimal Total, long CreatedAt);

static class OrderMapper
{
    // Opts lists only options that cost extra (extras, Large, Garlic rice), so defaults like "Regular" stay hidden.
    public static OrderDto ToDto(Order o) => new(
        o.Id, o.TableNumber,
        o.Allergies.Select(a => a.Name).ToList(),
        o.Items.Select(i => new OrderItemDto(
            i.MenuItem.Name, i.Quantity,
            i.Options.Where(x => x.PriceDelta > 0).Select(x => x.OptionName).ToList(),
            i.HasAllergyConflict)).ToList(),
        o.TotalAmount,
        new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds());
}