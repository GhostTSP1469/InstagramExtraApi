using InstagramExtraApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Стартовые данные, чтобы после деплоя список не был пустым.
        modelBuilder.Entity<Location>().HasData(
            new Location { LocationId = 1, City = "Dushanbe", State = "Dushanbe", ZipCode = "734000", Country = "Tajikistan" },
            new Location { LocationId = 2, City = "New York", State = "New York", ZipCode = "10001", Country = "USA" },
            new Location { LocationId = 3, City = "Moscow", State = "Moscow", ZipCode = "101000", Country = "Russia" }
        );
    }
}
