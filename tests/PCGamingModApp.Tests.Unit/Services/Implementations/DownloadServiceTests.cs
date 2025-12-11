using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;
using PCGamingModApp.Tests.Common;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadServiceTests : TestBase
{
    // private readonly IDownloadManager _downloadManager;
    private readonly Mock<IDownloadManager> _downloadManager;
    private readonly Mock<IDownloadRepository> _downloadRepository;
    private readonly IDownloadService _downloadService;

    public DownloadServiceTests()
    {
        Mock<IAppPaths> appPathsMock = new();
        Mock<IHttpClientFactory> httpClientFactoryMock = new();
        _downloadRepository = new();
        Mock<IMessenger> messengerMock = new();
        // _downloadManager = new DownloadManager(messengerMock.Object);
        _downloadManager = new();
        _downloadService = new DownloadService(appPathsMock.Object, httpClientFactoryMock.Object,
            _downloadRepository.Object, _downloadManager.Object, messengerMock.Object);
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
        await _downloadService.StartDownloadAsync(download.Id);

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
        await _downloadService.CancelDownloadAsync(download.Id);

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
        var remainingTime = _downloadService.GetRemainingTime(downloadId);

        // Assert
        Assert.Equal(expectedTime, remainingTime);
    }
}