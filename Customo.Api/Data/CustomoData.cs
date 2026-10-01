using Microsoft.EntityFrameworkCore;

namespace Customo.Data;

// ---------- Enums ----------
public enum OptionType { Size, Extra, Swap }

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

    public List<Allergen> Allergens { get; set; } = new();          // many-to-many
    public List<MenuItemOption> Options { get; set; } = new();
}

public class Allergen
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<MenuItem> MenuItems { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}

public class MenuItemOption
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public OptionType Type { get; set; }          // Size, Extra, or Swap
    public string Name { get; set; } = null!;
    public decimal PriceDelta { get; set; }       // 0 for "Regular"
}

// ---------- Order side (created when the customer submits) ----------
public class Order
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }      // recomputed on the server

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
    public bool HasAllergyConflict { get; set; }

    public List<OrderItemOption> Options { get; set; } = new();
}

public class OrderItemOption
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    public string OptionName { get; set; } = null!;   // snapshot, so menu edits don't change old orders
    public decimal PriceDelta { get; set; }
}

// ---------- DbContext ----------
public class CustomoDbContext : DbContext
{
    public CustomoDbContext(DbContextOptions<CustomoDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Allergen> Allergens => Set<Allergen>();
    public DbSet<MenuItemOption> MenuItemOptions => Set<MenuItemOption>();
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

        // Store the enum as text so the table is readable
        b.Entity<MenuItemOption>().Property(x => x.Type).HasConversion<string>();

        // Many-to-many join tables
        // MenuItem <-> Allergen join table, with the seed links (which dish contains which allergen)
        b.Entity<MenuItem>()
            .HasMany(m => m.Allergens).WithMany(a => a.MenuItems)
            .UsingEntity<Dictionary<string, object>>(
                "MenuItemAllergens",
                j => j.HasOne<Allergen>().WithMany().HasForeignKey("AllergenId"),
                j => j.HasOne<MenuItem>().WithMany().HasForeignKey("MenuItemId"),
                j =>
                {
                    j.HasKey("MenuItemId", "AllergenId");
                    j.HasData(MenuAllergenLinks.Select(l => new { MenuItemId = l.Item, AllergenId = l.Allergen }));
                });

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

        SeedMenu(b);
    }

    // ---------- Seed menu (12 sample dishes) ----------
    // Allergen ids: 1 Peanuts, 2 Tree nuts, 3 Milk, 4 Egg, 5 Wheat, 6 Soy, 7 Fish, 8 Shrimp, 9 Shellfish, 10 Sesame
    // (Item, Allergen) pairs: which dish contains which allergen
    private static readonly (int Item, int Allergen)[] MenuAllergenLinks =
    {
        (1, 6),                  // Chicken Adobo: soy
        (2, 5), (2, 4), (2, 8),  // Pancit Canton: wheat, egg, shrimp
        (3, 1), (3, 8),          // Kare-Kare: peanuts, shrimp
        (4, 4), (4, 6),          // Pork Sisig: egg, soy
        (5, 7),                  // Sinigang na Baboy: fish
        (6, 6),                  // Crispy Pata: soy
        (7, 5), (7, 4),          // Lumpiang Shanghai: wheat, egg
        (9, 3), (9, 1),          // Halo-Halo: milk, peanuts
        (10, 3), (10, 4),        // Leche Flan: milk, egg
        (12, 3),                 // Mango Shake: milk
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
