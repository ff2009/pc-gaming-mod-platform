using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadManagerTests
{
    [Fact]
    public void StartTracking_AddsDownloadToActiveDownloads()
    {
        // Arrange
        var messengerMock = new Mock<IMessenger>();
        var manager = new DownloadManager(messengerMock.Object);
        var download = new DownloadDataModel { Id = Guid.NewGuid() };

        // Act
        manager.StartTracking(download);

        // Assert
        Assert.Single(manager.GetActiveDownloads());
    }

    [Fact]
    public void UpdateProgress_UpdatesTrackerAndPublishesEvent()
    {
        // Arrange
        var messengerMock = new Mock<IMessenger>();
        var manager = new DownloadManager(messengerMock.Object);
        var download = new DownloadDataModel { Id = Guid.NewGuid(), FileSizeInBytes = 1000000 };
        manager.StartTracking(download);

        // Act
        manager.UpdateProgress(download.Id, 500000, 10000);

        // Assert
        messengerMock.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void StopTracking_RemovesDownloadAndPublishesEvent()
    {
        // Arrange
        var messengerMock = new Mock<IMessenger>();
        var manager = new DownloadManager(messengerMock.Object);
        var download = new DownloadDataModel { Id = Guid.NewGuid() };
        manager.StartTracking(download);

        // Act
        manager.StopTracking(download.Id);

        // Assert
        Assert.Empty(manager.GetActiveDownloads());
        messengerMock.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<string>()), Times.AtLeastOnce);
    }
}