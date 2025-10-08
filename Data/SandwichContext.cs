using Microsoft.EntityFrameworkCore;

namespace BackOfTheHouse.Data;

public class SandwichContext : DbContext
{
    public SandwichContext(DbContextOptions<SandwichContext> options) : base(options)
    {
    }

    public DbSet<Sandwich> Sandwiches => Set<Sandwich>();

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
            ctx.SaveChanges();
        }
    }
}
