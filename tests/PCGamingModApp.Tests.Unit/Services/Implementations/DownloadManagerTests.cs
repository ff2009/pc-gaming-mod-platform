using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadManagerTests
{
    private readonly Mock<IMessenger> _mockMessenger = new();
    private readonly DownloadManager _downloadManager;

    public DownloadManagerTests() 
    {
        _downloadManager = new DownloadManager(_mockMessenger.Object);
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
}