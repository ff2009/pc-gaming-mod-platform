using System.Collections;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;
using PCGamingModApp.Tests.Common;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class SingleDownloadServiceTests : TestBase
{
    // private readonly IDownloadManager _downloadManager;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IDownloadOrchestrator> _downloadOrchestrator = new();
    private readonly Mock<IDownloadRepository> _downloadRepository = new();
    private readonly Mock<IMessenger> _mockMessenger = new();
    
    private readonly ISingleDownloadService _singleDownloadService;

    public SingleDownloadServiceTests()
    {
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            Url = "http://example.com/file.zip",
            SavePath = "/downloads/file.zip",
            Status = Data.Enums.DownloadStatus.NotStarted
        };

        _singleDownloadService = new SingleDownloadService(
            download,
            _httpClientFactoryMock.Object,
            _downloadRepository.Object, 
            _mockMessenger.Object,
            () => 0, // getSpeedLimit
            (speed) => { }, // reportBandwidthUsage
            3);
    }

    [Fact]
    public async Task StartDownloadAsync_StartsTrackingDownload()
    {
        // Arrange
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            Url = "http://example.com/file.zip",
            SavePath = "/downloads/file.zip",
            Status = Data.Enums.DownloadStatus.NotStarted
        };

        _downloadOrchestrator.Setup(m => m.GetActiveDownloads()).Returns(new List<ISingleDownloadService>());
        _downloadRepository.Setup(m => m.GetDownloadById(download.Id)).ReturnsAsync(download);

        // Act
        await _singleDownloadService.StartDownloadAsync();

        // Assert
        _downloadOrchestrator.Verify(m => m.StartTracking(It.IsAny<DownloadDataModel>()), Times.Once);
        Assert.Single(_downloadOrchestrator.Object.GetActiveDownloads());
    }

    [Fact]
    public async Task CancelDownloadAsync_StopsTrackingDownload()
    {
        // Arrange
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            Url = "http://example.com/file.zip",
            SavePath = "/downloads/file.zip",
            Status = Data.Enums.DownloadStatus.InProgress
        };

        _downloadOrchestrator.Setup(m => m.GetActiveDownloads()).Returns(new List<ISingleDownloadService>());
        _downloadRepository.Setup(m => m.GetDownloadById(download.Id)).ReturnsAsync(download);

        // Act
        await _singleDownloadService.CancelDownloadAsync();

        // Assert
        _downloadOrchestrator.Verify(m => m.StopTracking(download.Id), Times.Once);
        Assert.Empty(_downloadOrchestrator.Object.GetActiveDownloads());
    }

    [Fact]
    public void GetRemainingTime_ReturnsTimeFromManager()
    {
        // Arrange
        var downloadId = Guid.NewGuid();
        var expectedTime = TimeSpan.FromSeconds(30);
        
        // Act
        var remainingTime = _singleDownloadService.GetRemainingTime();

        // Assert
        Assert.Equal(expectedTime, remainingTime);
    }
}