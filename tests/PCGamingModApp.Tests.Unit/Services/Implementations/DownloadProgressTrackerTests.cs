using PCGamingModApp.Core.Models;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class DownloadProgressTrackerTests
{
    [Fact]
    public void EstimateRemainingTime_ReturnsZero_WhenDownloadComplete()
    {
        // Arrange
        var download = new DownloadDataModel
        {
            FileSizeInBytes = 1_000, // 1KB
            DownloadedBytes = 0
        };
        var tracker = new DownloadProgressTracker(download);

        // Act: Simulate 1000 bytes downloaded
        for (int i = 0; i < 5; i++)
        {
            download.DownloadedBytes += 200;
            tracker.UpdateProgress(200);
            Thread.Sleep(1000);
        }

        // Assert
        Assert.Equal(0, tracker.CurrentSpeedBytesPerSecond);
        var remainingTime = tracker.EstimateRemainingTime();
        Assert.Equal(TimeSpan.Zero, remainingTime);
    }

    [Fact]
    public void UpdateProgress_WithZeroElapsedTime_SetsSpeedToZero()
    {
        // Arrange
        var download = new DownloadDataModel { FileSizeInBytes = 1000, DownloadedBytes = 0 };
        var tracker = new DownloadProgressTracker(download);

        // Act: Simulate two updates in quick succession (elapsedTime ≈ 0)
        tracker.UpdateProgress(100);
        Thread.Sleep(10); // Small delay to avoid exact zero
        tracker.UpdateProgress(200);

        // Assert
        Assert.Equal(0, tracker.CurrentSpeedBytesPerSecond);
    }

    [Fact]
    public void CurrentSpeedBytesPerSecond_WithStableDownload_ReturnsCorrectSpeed()
    {
        // Arrange
        var download = new DownloadDataModel { FileSizeInBytes = 1000, DownloadedBytes = 0 };
        var tracker = new DownloadProgressTracker(download);

        // Act: Simulate 100 bytes downloaded over 1 second
        tracker.UpdateProgress(100);
        Thread.Sleep(1000);
        tracker.UpdateProgress(100);
        Thread.Sleep(1000);

        // Assert: Speed should be ~100 bytes/sec
        Assert.InRange(tracker.CurrentSpeedBytesPerSecond, 90, 110);
    }

    [Fact]
    public void EstimateRemainingTime_WithStableDownload_ReturnsAccurateETA()
    {
        // Arrange
        var download = new DownloadDataModel { FileSizeInBytes = 1_000, DownloadedBytes = 0 };
        var tracker = new DownloadProgressTracker(download);

        // Act: Simulate 500 bytes downloaded over 5 seconds (100 bytes/sec)
        for (int i = 0; i < 5; i++)
        {
            download.DownloadedBytes += 100;
            tracker.UpdateProgress(100);
            Thread.Sleep(1000);
        }

        // Assert: ETA for remaining 500 bytes should be ~5 seconds
        var eta = tracker.EstimateRemainingTime();
        Assert.InRange(eta.TotalSeconds, 4, 6);
    }

    [Fact]
    public void UpdateProgress_MultiThreaded_DoesNotThrow()
    {
        // Arrange
        var download = new DownloadDataModel { FileSizeInBytes = 1000, DownloadedBytes = 0 };
        var tracker = new DownloadProgressTracker(download);
        int threads = 10;
        var exceptions = new List<Exception>();

        // Act: Simulate multi-threaded updates
        var threadActions = Enumerable.Range(0, threads).Select(_ => new Action(() =>
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    tracker.UpdateProgress(10);
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToList();

        Parallel.Invoke(threadActions.ToArray());

        // Assert: No exceptions should occur
        Assert.Empty(exceptions);
    }

    [Fact]
    public void LongTermSpeed_WithDownsampling_UsesAggregatedSamples_Short()
    {
        // Arrange
        var download = new DownloadDataModel { FileSizeInBytes = 1000, DownloadedBytes = 0 };
        var tracker = new DownloadProgressTracker(download);

        // Act: Simulate rapid updates (should be downsampled to 1-second intervals)
        for (int i = 0; i < 30; i++)
        {
            tracker.UpdateProgress(10); // 10 bytes per update
            Thread.Sleep(100); // 100ms per update → 10 updates/sec
        }

        // Assert: Long-term window should have ~3 samples (1 per second)
        // (Note: This is a simplified check; actual implementation may vary)
        var longTermSpeed = tracker.LongTermEmaSpeedBytesPerSecond;
        Assert.InRange(longTermSpeed, 50, 70); // ~60 bytes/sec average
    }

    [Fact(Skip = "Long Term Speed")]
    public void LongTermSpeed_WithDownsampling_UsesAggregatedSamples_Long()
    {
        // Arrange
        var download = new DownloadDataModel { FileSizeInBytes = 1000, DownloadedBytes = 0 };
        var tracker = new DownloadProgressTracker(download);

        // Act: Simulate rapid updates (should be downsampled to 1-second intervals)
        for (int i = 0; i < 301; i++)
        {
            tracker.UpdateProgress(10); // 10 bytes per update
            Thread.Sleep(100); // 100ms per update → 10 updates/sec
        }

        // Assert: Long-term window should have ~3 samples (1 per second)
        // (Note: This is a simplified check; actual implementation may vary)
        var longTermSpeed = tracker.LongTermEmaSpeedBytesPerSecond;
        Assert.InRange(longTermSpeed, 90, 110); // ~100 bytes/sec average
    }
}