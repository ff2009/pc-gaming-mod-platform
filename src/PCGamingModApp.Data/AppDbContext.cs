using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<GameDataModel> Games { get; set; }
}