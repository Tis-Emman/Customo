using Microsoft.EntityFrameworkCore;

namespace Customo.Data;

// ---------- Enums ----------
// Remove is used only on order items, to record an ingredient the customer left out
public enum OptionType { Size, Extra, Swap, Remove }

// ---------- Menu side (preloaded with seed data) ----------
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<MenuItem> MenuItems { get; set; } = new();
}

public class MenuItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    // A dish's allergens come from its ingredients
    public List<MenuItemIngredient> Ingredients { get; set; } = new();
    public List<MenuItemOption> Options { get; set; } = new();
}

public class Allergen
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Order> Orders { get; set; } = new();
}

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? AllergenId { get; set; }          // null when the ingredient has no common allergen
    public Allergen? Allergen { get; set; }
}

// Which ingredients a dish contains, and whether the customer may leave one out
public class MenuItemIngredient
{
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public bool IsRemovable { get; set; }
}

public class MenuItemOption
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public OptionType Type { get; set; }          // Size, Extra, or Swap
    public string Name { get; set; } = null!;
    public decimal PriceDelta { get; set; }       // 0 for the default ("Regular", "Steamed rice")
}

// ---------- Order side (created when the customer submits) ----------
public class Order
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }      // recomputed on the server
    public DateTime? CompletedAt { get; set; }    // null = still active on the kitchen screen

    public List<Allergen> Allergies { get; set; } = new();   // customer's declared allergies
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }        // snapshot of the price at order time
    public bool HasAllergyConflict { get; set; }  // true if an allergen is still in the dish after removals

    public List<OrderItemOption> Options { get; set; } = new();
}

public class OrderItemOption
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    public OptionType Type { get; set; }              // Remove = an ingredient left out
    public string OptionName { get; set; } = null!;   // snapshot, so menu edits don't change old orders
    public decimal PriceDelta { get; set; }
}

// ---------- Tables ----------
public class DiningTable
{
    public int Number { get; set; }             // the table number is the key
    public string? ClaimToken { get; set; }     // secret kept in the customer's browser
    public DateTime? OccupiedAt { get; set; }   // last activity; the hold expires after 3 hours
}

// ---------- DbContext ----------
public class CustomoDbContext : DbContext
{
    public CustomoDbContext(DbContextOptions<CustomoDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Allergen> Allergens => Set<Allergen>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<MenuItemIngredient> MenuItemIngredients => Set<MenuItemIngredient>();
    public DbSet<MenuItemOption> MenuItemOptions => Set<MenuItemOption>();
    public DbSet<DiningTable> Tables => Set<DiningTable>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemOption> OrderItemOptions => Set<OrderItemOption>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Money columns
        b.Entity<MenuItem>().Property(x => x.Price).HasPrecision(10, 2);
        b.Entity<MenuItemOption>().Property(x => x.PriceDelta).HasPrecision(10, 2);
        b.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(10, 2);
        b.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(10, 2);
        b.Entity<OrderItemOption>().Property(x => x.PriceDelta).HasPrecision(10, 2);

        // Store the enums as text so the tables are readable
        b.Entity<MenuItemOption>().Property(x => x.Type).HasConversion<string>();
        b.Entity<OrderItemOption>().Property(x => x.Type).HasConversion<string>();

        // Dish <-> Ingredient link table (composite key)
        b.Entity<MenuItemIngredient>().HasKey(x => new { x.MenuItemId, x.IngredientId });
        b.Entity<Ingredient>().HasOne(i => i.Allergen).WithMany().HasForeignKey(i => i.AllergenId);

        // Order <-> Allergen join table
        b.Entity<Order>()
            .HasMany(o => o.Allergies).WithMany(a => a.Orders)
            .UsingEntity(j => j.ToTable("OrderAllergies"));

        // Seed data: allergens (matches the allergy picker screen)
        b.Entity<Allergen>().HasData(
            new Allergen { Id = 1, Name = "Peanuts" },
            new Allergen { Id = 2, Name = "Tree nuts" },
            new Allergen { Id = 3, Name = "Milk" },
            new Allergen { Id = 4, Name = "Egg" },
            new Allergen { Id = 5, Name = "Wheat" },
            new Allergen { Id = 6, Name = "Soy" },
            new Allergen { Id = 7, Name = "Fish" },
            new Allergen { Id = 8, Name = "Shrimp" },
            new Allergen { Id = 9, Name = "Shellfish" },
            new Allergen { Id = 10, Name = "Sesame" }
        );

        // Tables 1 to 12, all free at the start
        b.Entity<DiningTable>().HasKey(t => t.Number);
        b.Entity<DiningTable>().Property(t => t.Number).ValueGeneratedNever();
        b.Entity<DiningTable>().HasData(Enumerable.Range(1, 12).Select(n => new DiningTable { Number = n }));

        SeedMenu(b);
    }

    // ---------- Seed menu (12 sample dishes) ----------
    // Allergen ids: 1 Peanuts, 2 Tree nuts, 3 Milk, 4 Egg, 5 Wheat, 6 Soy, 7 Fish, 8 Shrimp, 9 Shellfish, 10 Sesame
    private static Ingredient Ing(int id, string name, int? allergenId = null) =>
        new() { Id = id, Name = name, AllergenId = allergenId };

    // (Dish, Ingredient, Removable): false = the customer cannot leave it out
    private static readonly (int Item, int Ing, bool Removable)[] DishIngredients =
    {
        (1, 1, false), (1, 2, false), (1, 3, false), (1, 4, true), (1, 5, true), (1, 6, true),            // Chicken Adobo
        (2, 7, false), (2, 1, true), (2, 8, true), (2, 9, true), (2, 10, true), (2, 11, true), (2, 4, true), // Pancit Canton
        (3, 12, false), (3, 13, false), (3, 14, true), (3, 15, true), (3, 16, true), (3, 17, true),      // Kare-Kare
        (4, 18, false), (4, 2, false), (4, 9, true), (4, 19, true), (4, 20, true), (4, 21, true),         // Pork Sisig
        (5, 18, false), (5, 22, false), (5, 23, true), (5, 24, true), (5, 25, true), (5, 26, true),       // Sinigang na Baboy
        (6, 43, false), (6, 2, true), (6, 4, true),                                                      // Crispy Pata
        (7, 18, false), (7, 27, false), (7, 9, false), (7, 11, true), (7, 19, true),                      // Lumpiang Shanghai
        (8, 28, false),                                                                                   // Plain Rice
        (9, 29, false), (9, 30, true), (9, 31, true), (9, 32, true), (9, 33, true),                       // Halo-Halo
        (10, 35, false), (10, 34, false), (10, 36, false),                                                // Leche Flan
        (11, 37, false), (11, 38, true), (11, 39, true), (11, 40, true),                                  // Iced Tea
        (12, 41, false), (12, 30, true), (12, 39, true), (12, 42, true),                                  // Mango Shake
    };

    private static void SeedMenu(ModelBuilder b)
    {
        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Mains" },
            new Category { Id = 2, Name = "Sides" },
            new Category { Id = 3, Name = "Desserts" },
            new Category { Id = 4, Name = "Drinks" });

        b.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, CategoryId = 1, Name = "Chicken Adobo", Price = 150 },
            new MenuItem { Id = 2, CategoryId = 1, Name = "Pancit Canton", Price = 120 },
            new MenuItem { Id = 3, CategoryId = 1, Name = "Kare-Kare", Price = 250 },
            new MenuItem { Id = 4, CategoryId = 1, Name = "Pork Sisig", Price = 180 },
            new MenuItem { Id = 5, CategoryId = 1, Name = "Sinigang na Baboy", Price = 200 },
            new MenuItem { Id = 6, CategoryId = 1, Name = "Crispy Pata", Price = 450 },
            new MenuItem { Id = 7, CategoryId = 2, Name = "Lumpiang Shanghai", Price = 100 },
            new MenuItem { Id = 8, CategoryId = 2, Name = "Plain Rice", Price = 30 },
            new MenuItem { Id = 9, CategoryId = 3, Name = "Halo-Halo", Price = 110 },
            new MenuItem { Id = 10, CategoryId = 3, Name = "Leche Flan", Price = 80 },
            new MenuItem { Id = 11, CategoryId = 4, Name = "Iced Tea", Price = 60 },
            new MenuItem { Id = 12, CategoryId = 4, Name = "Mango Shake", Price = 90 });

        b.Entity<Ingredient>().HasData(
            Ing(1, "Chicken"), Ing(2, "Soy sauce", 6), Ing(3, "Vinegar"), Ing(4, "Garlic"), Ing(5, "Bay leaf"),
            Ing(6, "Black pepper"), Ing(7, "Canton noodles", 5), Ing(8, "Shrimp", 8), Ing(9, "Egg", 4),
            Ing(10, "Cabbage"), Ing(11, "Carrots"), Ing(12, "Beef"), Ing(13, "Peanut sauce", 1),
            Ing(14, "Shrimp paste", 8), Ing(15, "Eggplant"), Ing(16, "String beans"), Ing(17, "Bok choy"),
            Ing(18, "Pork"), Ing(19, "Onions"), Ing(20, "Chili"), Ing(21, "Calamansi"),
            Ing(22, "Tamarind broth"), Ing(23, "Fish sauce", 7), Ing(24, "Radish"), Ing(25, "Kangkong"),
            Ing(26, "Tomato"), Ing(27, "Lumpia wrapper", 5), Ing(28, "Rice"), Ing(29, "Shaved ice"),
            Ing(30, "Milk", 3), Ing(31, "Sweet beans"), Ing(32, "Banana"), Ing(33, "Peanut topping", 1),
            Ing(34, "Condensed milk", 3), Ing(35, "Egg yolks", 4), Ing(36, "Caramel"), Ing(37, "Tea"),
            Ing(38, "Sugar syrup"), Ing(39, "Ice"), Ing(40, "Lemon"), Ing(41, "Mango"), Ing(42, "Sugar"),
            Ing(43, "Pork leg"));

        b.Entity<MenuItemIngredient>().HasData(
            DishIngredients.Select(l => new MenuItemIngredient { MenuItemId = l.Item, IngredientId = l.Ing, IsRemovable = l.Removable }));

        b.Entity<MenuItemOption>().HasData(
            Opt(1, 1, OptionType.Extra, "Extra rice", 20),
            Opt(2, 1, OptionType.Extra, "Egg", 15),
            Opt(3, 1, OptionType.Swap, "Steamed rice", 0),
            Opt(4, 1, OptionType.Swap, "Garlic rice", 10),
            Opt(5, 2, OptionType.Extra, "Extra veggies", 20),
            Opt(6, 3, OptionType.Extra, "Extra bagoong", 10),
            Opt(7, 4, OptionType.Extra, "Extra rice", 20),
            Opt(8, 4, OptionType.Extra, "Egg", 15),
            Opt(9, 5, OptionType.Extra, "Extra rice", 20),
            Opt(10, 6, OptionType.Extra, "Extra sauce", 15),
            Opt(11, 7, OptionType.Extra, "Sweet chili", 10),
            Opt(12, 9, OptionType.Extra, "Ice cream", 30),
            Opt(13, 9, OptionType.Extra, "Leche flan", 25),
            Opt(14, 11, OptionType.Size, "Regular", 0),
            Opt(15, 11, OptionType.Size, "Medium", 10),
            Opt(16, 11, OptionType.Size, "Large", 20),
            Opt(17, 12, OptionType.Size, "Regular", 0),
            Opt(18, 12, OptionType.Size, "Medium", 10),
            Opt(19, 12, OptionType.Size, "Large", 20));
    }

    private static MenuItemOption Opt(int id, int menuItemId, OptionType type, string name, decimal delta) =>
        new() { Id = id, MenuItemId = menuItemId, Type = type, Name = name, PriceDelta = delta };
}
