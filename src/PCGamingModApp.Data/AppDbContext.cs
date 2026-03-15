using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DownloadDataModel> Downloads { get; set; }
    public DbSet<GameDataModel> Games { get; set; }
    public DbSet<DownloadSettings> DownloadSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Downloads
        modelBuilder.Entity<DownloadDataModel>()
            .HasKey(f => f.Id);
        
        // Games
        modelBuilder.Entity<GameDataModel>()
            .HasKey(f => f.Id);
            
        // Download Settings
        modelBuilder.Entity<DownloadSettings>()
            .HasKey(s => s.Id);
    }
}