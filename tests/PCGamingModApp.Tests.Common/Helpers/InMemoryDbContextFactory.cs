using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data;

namespace PCGamingModApp.Tests.Common.Helpers;

public static class InMemoryDbContextFactory
{
    public static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test
            .Options;

        var context = new AppDbContext(options);
        return context;
    }
}