using System.Net;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;
using PCGamingModApp.Tests.Common.Helpers;

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
        _appPaths.SetupGet(a => a.Downloads).Returns(@"C:\Downloads");

        _downloadManager = new DownloadManager(
            _appPaths.Object,
            _httpClientFactoryMock.Object,
            _downloadRepository.Object,
            _mockMessenger.Object);
    }

    [Fact]
    public async Task GetDownloadMetadataAsync_ReturnsExpectedMetadata()
    {
        // Arrange
        var url = "https://example.com/files/testfile.zip";
        var expectedLength = 12345L;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[expectedLength])
        };

        var mockHandler = new MockHttpMessageHandler((req, ct) =>
            Task.FromResult(response));
        
        var httpClient = new HttpClient(mockHandler);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var metadata = await _downloadManager.GetDownloadMetadataAsync(url);

        // Assert
        Assert.Equal(url, metadata.Url);
        Assert.Equal("testfile.zip", metadata.FileName);
        Assert.Equal(Path.Combine(@"C:\Downloads", "testfile.zip"), metadata.SavePath);
        Assert.Equal(expectedLength, metadata.FileSizeInBytes);
        Assert.Equal(DownloadStatus.Pending, metadata.Status);
        Assert.True((DateTime.Now - metadata.CreatedAt) < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetDownloadMetadataAsync_ThrowsWhenResponseNotSuccess()
    {
        // Arrange
        const string url = "https://example.com/files/missing.zip";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(string.Empty)
        };

        var mockHandler = new MockHttpMessageHandler((req, ct) =>
            Task.FromResult(response));
        
        var httpClient = new HttpClient(mockHandler);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);


        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _downloadManager.GetDownloadMetadataAsync(url));
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