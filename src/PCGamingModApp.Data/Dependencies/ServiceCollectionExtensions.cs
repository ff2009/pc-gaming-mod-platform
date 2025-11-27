using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Data.Dependencies;

public static class ServiceCollectionExtensions
{
    public static void AddDataRepository(this IServiceCollection services,
        string connectionString = "DataSource=pcgamingmod.db")
    {
        services.AddScoped<IDownloadRepository, DownloadRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));
    }
    
    public static void InitializeDatabase(this ServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
    }
}