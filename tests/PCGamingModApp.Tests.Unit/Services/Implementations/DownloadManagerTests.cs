using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadManagerTests
{
    private readonly Mock<IAppPaths> _appPaths = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IDownloadRepository> _downloadRepository = new();
    private readonly Mock<IMessenger> _mockMessenger = new();
    
    private readonly DownloadManager _downloadManager;

    public DownloadManagerTests()
    {
        _downloadManager = new DownloadManager(
            _appPaths.Object,
            _httpClientFactoryMock.Object,
            _downloadRepository.Object,
            _mockMessenger.Object);
    }

    [Fact]
    public void GetDownloadsAsync_ReturnsActiveDownloads()
    {
        // Arrange
        var download1 = new DownloadDataModel { Id = Guid.NewGuid() };
        var download2 = new DownloadDataModel { Id = Guid.NewGuid() };
        _downloadManager.StartTracking(download1);
        _downloadManager.StartTracking(download2);

        // Act
        var downloads = _downloadManager.GetActiveDownloads();

        // Assert
        Assert.Multiple(() => { Assert.Equal(2, downloads.Count); });
        Assert.Contains(downloads, m => m.Equals(download1.Id));
    }

    [Fact]
    public void GetDownloadsAsync_ReturnsEmptyList_WhenNoDownloadsExist()
    {
        // Act
        var downloads = _downloadManager.GetActiveDownloads();

        // Assert
        Assert.Empty(downloads);
    }

    [Fact]
    public void StartTracking_AddsDownloadToActiveDownloads()
    {
        // Arrange
        var download = new DownloadDataModel { Id = Guid.NewGuid() };

        // Act
        _downloadManager.StartTracking(download);

        // Assert
        Assert.Single(_downloadManager.GetActiveDownloads());
    }

    [Fact]
    public void StartTracking_DoesNotAddDuplicateDownload()
    {
        // Arrange
        var download = new DownloadDataModel { Id = Guid.NewGuid() };

        _downloadManager.StartTracking(download);

        // Act
        _downloadManager.StartTracking(download);

        // Assert
        Assert.Single(_downloadManager.GetActiveDownloads());
    }
}