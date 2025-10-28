using Microsoft.EntityFrameworkCore;

namespace BackOfTheHouse.Data;

public class SandwichContext : DbContext
{
    public SandwichContext(DbContextOptions<SandwichContext> options) : base(options)
    {
    }

    public DbSet<Sandwich> Sandwiches => Set<Sandwich>();
    // Lightweight option set used only for the SQLite fallback in development.
    // We store all options in a single table with a Category so we can easily
    // query per kind (breads/cheeses/etc.).
    public DbSet<Option> Options => Set<Option>();

    public static void EnsureSeedData(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SandwichContext>();
        ctx.Database.EnsureCreated();
        if (!ctx.Sandwiches.Any())
        {
            ctx.Sandwiches.AddRange(
                new Sandwich { Name = "BLT", Description = "Bacon, Lettuce, Tomato", Price = 6.99m },
                new Sandwich { Name = "Turkey Club", Description = "Turkey, Bacon, Lettuce", Price = 8.49m },
                new Sandwich { Name = "Veggie", Description = "Grilled Veggies and Hummus", Price = 7.25m }
            );
        }

        // Seed simple option lookup tables when running on SQLite to provide
        // reasonable defaults for the builder UI in local dev. If the existing
        // DB file was created before these tables were added, queries like
        // `ctx.Breads.Any()` will throw. Catch any exception here and skip
        // seeding so the application continues to run (options will be empty).
        try
        {
            if (!ctx.Options.Any())
            {
                // Breads
                ctx.Options.AddRange(
                    new Option { Name = "White", Category = "breads" },
                    new Option { Name = "Wheat", Category = "breads" },
                    new Option { Name = "Sourdough", Category = "breads" },
                    // Cheeses
                    new Option { Name = "Cheddar", Category = "cheeses" },
                    new Option { Name = "Swiss", Category = "cheeses" },
                    new Option { Name = "Provolone", Category = "cheeses" },
                    // Dressings
                    new Option { Name = "Mayo", Category = "dressings" },
                    new Option { Name = "Mustard", Category = "dressings" },
                    new Option { Name = "Ranch", Category = "dressings" },
                    // Meats
                    new Option { Name = "Turkey", Category = "meats" },
                    new Option { Name = "Ham", Category = "meats" },
                    new Option { Name = "Bacon", Category = "meats" },
                    new Option { Name = "Tempeh", Category = "meats" },
                    // Toppings
                    new Option { Name = "Lettuce", Category = "toppings" },
                    new Option { Name = "Tomato", Category = "toppings" },
                    new Option { Name = "Onion", Category = "toppings" }
                );
            }

            ctx.SaveChanges();
        }
        catch
        {
            // If any query fails (for example the Option table doesn't exist in
            // an older DB), skip seeding options. The app will still function
            // and the frontend will receive empty arrays.
        }
    }
}

// Lightweight option entity used only for SQLite fallback. The scaffolded
// Docker context has fully typed option tables; we reuse a tiny Option here
// to seed data without needing the scaffolded models.
public class Option
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Category values mirror the API route names: "breads", "cheeses",
    // "dressings", "meats", "toppings". This lets the SQLite fallback
    // store options in a single table and still return per-kind lists.
    public string Category { get; set; } = string.Empty;
}
