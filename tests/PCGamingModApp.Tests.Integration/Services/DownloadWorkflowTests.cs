using System.Net;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;
using PCGamingModApp.Tests.Utilities.Helpers;
using PCGamingModApp.Tests.Utilities.Repositories;
using Xunit;

namespace PCGamingModApp.Tests.Integration.Services;

public class DownloadWorkflowTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();


    [Fact]
    public async Task DownloadWorkflow_StartUpdateStop_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockHandler = new MockHttpMessageHandler((req, ct) =>
            Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"key\":\"value\"}"),
            }));

        var httpClient = new HttpClient(mockHandler);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory
            .Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        services.AddSingleton<IAppPaths, AppPaths>();
        services.AddSingleton<DownloadManager>();
        services.AddSingleton<IHttpClientFactory>(mockHttpClientFactory.Object);
        services.AddTransient<IDownloadRepository, MockDownloadRepository>();
        services.AddTransient<IDownloadService, DownloadService>();
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        var serviceProvider = services.BuildServiceProvider();
        var downloadService = serviceProvider.GetRequiredService<IDownloadService>();
        var downloadManager = serviceProvider.GetRequiredService<DownloadManager>();

        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            FileSizeInBytes = 1000000,
            DownloadedBytes = 0,
            Status = DownloadStatus.Pending,
        };

        // Act
        await downloadService.StartDownloadAsync(download.Id);
        downloadManager.UpdateProgress(download.Id, 500000, 10000);
        await downloadService.CancelDownloadAsync(download.Id);

        // Assert
        Assert.Empty(downloadManager.GetActiveDownloads());
    }
}