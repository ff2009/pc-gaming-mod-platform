using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Tests.Common;
using PCGamingModApp.Tests.Common.Helpers;
using Xunit;

namespace PCGamingModApp.Tests.Integration.Services;

public class DownloadWorkflowTests : TestBase
{
    private readonly IDownloadManager _downloadManager;
    private readonly IDownloadService _downloadService;

    public DownloadWorkflowTests()
    {
        _downloadService = ServiceProvider.GetRequiredService<IDownloadService>();
        _downloadManager = ServiceProvider.GetRequiredService<IDownloadManager>();
    }


    [Fact]
    public async Task DownloadWorkflow_StartUpdateStop_Succeeds()
    {
        // Arrange   
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
        
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            FileSizeInBytes = 1000000,
            DownloadedBytes = 0,
            Status = DownloadStatus.Pending,
        };

        // Act
        await _downloadService.StartDownloadAsync(download.Id);
        _downloadManager.UpdateProgress(download.Id, 500000, 10000);
        await _downloadService.CancelDownloadAsync(download.Id);

        // Assert
        Assert.Empty(_downloadManager.GetActiveDownloads());
    }
}