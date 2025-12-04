using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadServiceTests
{
    [Fact]
    public async Task StartDownloadAsync_StartsTrackingDownload()
    {
        // Arrange
        var appPathsMock = new Mock<AppPaths>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var downloadRepositoryMock = new Mock<IDownloadRepository>();
        var managerMock = new Mock<DownloadManager>();
        var messengerMock = new Mock<IMessenger>();
        var service = new DownloadService(appPathsMock.Object, httpClientFactoryMock.Object,
            downloadRepositoryMock.Object, managerMock.Object, messengerMock.Object);
        var download = new DownloadDataModel { Id = Guid.NewGuid() };

        // Act
        await service.StartDownloadAsync(download.Id);

        // Assert
        managerMock.Verify(m => m.StartTracking(It.IsAny<DownloadDataModel>()), Times.Once);
    }

    [Fact]
    public async Task CancelDownloadAsync_StopsTrackingDownload()
    {
        // Arrange
        var appPathsMock = new Mock<AppPaths>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var downloadRepositoryMock = new Mock<IDownloadRepository>();
        var managerMock = new Mock<DownloadManager>();
        var messengerMock = new Mock<IMessenger>();
        var service = new DownloadService(appPathsMock.Object, httpClientFactoryMock.Object,
            downloadRepositoryMock.Object, managerMock.Object, messengerMock.Object);
        var downloadId = Guid.NewGuid();

        // Act
        await service.CancelDownloadAsync(downloadId);

        // Assert
        managerMock.Verify(m => m.StopTracking(downloadId), Times.Once);
    }

    [Fact]
    public void GetRemainingTime_ReturnsTimeFromManager()
    {
        // Arrange
        var appPathsMock = new Mock<AppPaths>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var downloadRepositoryMock = new Mock<IDownloadRepository>();
        var managerMock = new Mock<DownloadManager>();
        var messengerMock = new Mock<IMessenger>();
        var service = new DownloadService(appPathsMock.Object, httpClientFactoryMock.Object,
            downloadRepositoryMock.Object, managerMock.Object, messengerMock.Object);
        var downloadId = Guid.NewGuid();
        var expectedTime = TimeSpan.FromSeconds(30);
        managerMock.Setup(m => m.GetRemainingTime(downloadId)).Returns(expectedTime);

        // Act
        var remainingTime = service.GetRemainingTime(downloadId);

        // Assert
        Assert.Equal(expectedTime, remainingTime);
    }
}