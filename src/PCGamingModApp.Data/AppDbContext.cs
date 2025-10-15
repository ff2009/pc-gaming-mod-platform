using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GameDataModel> Games { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Games
        modelBuilder.Entity<GameDataModel>()
            .HasKey(f => f.Id);
    }
}