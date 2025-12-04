using PCGamingModApp.Core.Models;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadProgressTrackerTests
{
    [Fact]
    public void UpdateProgress_UpdatesEmaSpeedCorrectly()
    {
        // Arrange
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            FileSizeInBytes = 1000000, // 1MB
            DownloadedBytes = 0,
        };
        var tracker = new DownloadProgressTracker(download);

        // Act
        tracker.UpdateProgress(100000, 10000); // 10KB downloaded, 10KB/s speed
        tracker.UpdateProgress(200000, 20000); // 20KB downloaded, 20KB/s speed

        // Assert
        Assert.InRange(tracker.EstimateRemainingTime().TotalSeconds, 30, 40);
    }

    [Fact]
    public void EstimateRemainingTime_ReturnsZero_WhenDownloadComplete()
    {
        // Arrange
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            FileSizeInBytes = 1000000,
            DownloadedBytes = 1000000,
        };
        var tracker = new DownloadProgressTracker(download);

        // Act
        var remainingTime = tracker.EstimateRemainingTime();

        // Assert
        Assert.Equal(TimeSpan.Zero, remainingTime);
    }

    [Fact]
    public void EstimateRemainingTime_ReturnsZero_WhenSpeedIsZero()
    {
        // Arrange
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            FileSizeInBytes = 1000000,
            DownloadedBytes = 500000,
        };
        var tracker = new DownloadProgressTracker(download);

        // Act
        tracker.UpdateProgress(500000, 0); // Zero speed
        var remainingTime = tracker.EstimateRemainingTime();

        // Assert
        Assert.Equal(TimeSpan.Zero, remainingTime);
    }
}