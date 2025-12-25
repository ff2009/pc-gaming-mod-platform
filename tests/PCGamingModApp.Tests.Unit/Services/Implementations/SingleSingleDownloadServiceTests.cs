using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;
using PCGamingModApp.Tests.Common;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class SingleSingleDownloadServiceTests : TestBase
{
    // private readonly IDownloadManager _downloadManager;
    private readonly  Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IDownloadOrchestrator> _downloadManager = new();
    private readonly Mock<IDownloadRepository> _downloadRepository = new();
    private readonly Mock<IMessenger> _mockMessenger = new();
    
    private readonly ISingleDownloadService _singleDownloadService;

    public SingleSingleDownloadServiceTests()
    {
        _singleDownloadService = new SingleSingleDownloadService(
            _httpClientFactoryMock.Object,
            _downloadManager.Object,
            _downloadRepository.Object, 
            _mockMessenger.Object);
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

        _downloadManager.Setup(m => m.GetActiveDownloads()).Returns(new List<Guid> { download.Id });
        _downloadRepository.Setup(m => m.GetDownloadById(download.Id)).ReturnsAsync(download);

        // Act
        await _singleDownloadService.StartDownloadAsync(download.Id);

        // Assert
        _downloadManager.Verify(m => m.StartTracking(It.IsAny<DownloadDataModel>()), Times.Once);
        Assert.Single(_downloadManager.Object.GetActiveDownloads());
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

        _downloadManager.Setup(m => m.GetActiveDownloads()).Returns(new List<Guid>());
        _downloadRepository.Setup(m => m.GetDownloadById(download.Id)).ReturnsAsync(download);

        // Act
        await _singleDownloadService.CancelDownloadAsync(download.Id);

        // Assert
        _downloadManager.Verify(m => m.StopTracking(download.Id), Times.Once);
        Assert.Empty(_downloadManager.Object.GetActiveDownloads());
    }

    [Fact]
    public void GetRemainingTime_ReturnsTimeFromManager()
    {
        // Arrange
        var downloadId = Guid.NewGuid();
        var expectedTime = TimeSpan.FromSeconds(30);
        _downloadManager.Setup(m => m.GetRemainingTime(downloadId)).Returns(expectedTime);

        // Act
        var remainingTime = _singleDownloadService.GetRemainingTime(downloadId);

        // Assert
        Assert.Equal(expectedTime, remainingTime);
    }
}