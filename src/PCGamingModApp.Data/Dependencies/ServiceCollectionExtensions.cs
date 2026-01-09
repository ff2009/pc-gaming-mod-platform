using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Data.Dependencies;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddDataRepository(string connectionString = "DataSource=pcgamingmod.db")
        {
            services.AddScoped<IDownloadRepository, DownloadRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));
        }
    }

    extension(ServiceProvider serviceProvider)
    {
        public void InitializeDatabase()
        {
            var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}