using System.Net;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data;
using PCGamingModApp.Data.Repositories;
using PCGamingModApp.Tests.Common.Helpers;

namespace PCGamingModApp.Tests.Common;

public class TestBase
{
    protected ServiceProvider ServiceProvider { get; }

    protected TestBase()
    {
        var services = new ServiceCollection();

        // Register DbContext for integration tests
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Register services for integration tests
        services.AddTransient<IDownloadService, DownloadService>();
        services.AddSingleton<IAppPaths, AppPaths>();
        
        var mockHandler = new MockHttpMessageHandler((req, ct) =>
            Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"key\":\"value\"}"),
            }));

        var httpClient = new HttpClient(mockHandler);
        
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory
            .Setup(m => m.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
        
        services.AddSingleton(mockHttpClientFactory.Object);
        
        services.AddSingleton<IDownloadManager, DownloadManager>();
        services.AddTransient<IDownloadRepository, DownloadRepository>();
        services.AddTransient<IDownloadService, DownloadService>();
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        
        ServiceProvider = services.BuildServiceProvider();
    }
}