using System.Diagnostics;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Models;

public class DownloadProgressTracker(DownloadDataModel download)
{
    private const double SmoothingFactor = 0.2; // Adjust as needed

    /// <summary>
    /// Exponential Moving Average (EMA)
    /// </summary>
    private long _emaSpeed = 0;

    /// <summary>
    /// Total time elapsed since the download started
    /// </summary>
    private Stopwatch _stopwatch = Stopwatch.StartNew();
    
    /// <summary>
    /// Current download speed in bytes per second
    /// </summary>
    public long CurrentSpeedBytesPerSecond { get; private set; } = 0;
    
    private const int WindowSize = 10; // 10 seconds
    private Queue<(long Bytes, DateTime Timestamp)> _speedWindow = new();

    /// <summary>
    /// Updates the progress and recalculates the average speed over the last 60 seconds.
    /// </summary>
    /// <param name="downloadedBytes">Current downloaded bytes.</param>
    public void UpdateProgress(long downloadedBytes)
    {
        _speedWindow.Enqueue((downloadedBytes, DateTime.Now));

        // Remove samples outside the window
        while (_speedWindow.Count > 0 && (DateTime.Now - _speedWindow.Peek().Timestamp).TotalSeconds > WindowSize)
            _speedWindow.Dequeue();

        // Calculate the elapsed time
        var elapsedTime = (DateTime.Now - _speedWindow.Peek().Timestamp).TotalSeconds;

        // Calculate the average speed over the last 60 seconds
        CurrentSpeedBytesPerSecond = (long)(_speedWindow.Sum(s => s.Bytes) / elapsedTime);

        if (_emaSpeed == 0)
            _emaSpeed = CurrentSpeedBytesPerSecond; // Initialize EMA
        else
            _emaSpeed = (long)(CurrentSpeedBytesPerSecond * SmoothingFactor + _emaSpeed * (1 - SmoothingFactor));
    }

    /// <summary>
    /// Estimates the remaining time for the download.
    /// </summary>
    /// <returns>TimeSpan representing remaining time.</returns>
    public TimeSpan EstimateRemainingTime()
    {
        if (_emaSpeed <= 0 || download.FileSizeInBytes <= 0)
            return TimeSpan.Zero;

        long remainingBytes = download.FileSizeInBytes - download.DownloadedBytes;
        return TimeSpan.FromSeconds(remainingBytes / _emaSpeed);
    }
}