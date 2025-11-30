using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Models;

public class DownloadProgressTracker(DownloadDataModel download)
{
    /// <summary>
    /// Exponential Moving Average (EMA)
    /// </summary>
    private long _emaSpeed = 0;
    private const double SmoothingFactor = 0.2; // Adjust as needed

    /// <summary>
    /// Updates the progress and recalculates EMA speed.
    /// </summary>
    /// <param name="downloadedBytes">Current downloaded bytes.</param>
    /// <param name="currentSpeed">Current download speed in bytes/second.</param>
    public void UpdateProgress(long downloadedBytes, long currentSpeed)
    {
        download.DownloadedBytes = downloadedBytes;
        if (_emaSpeed == 0)
            _emaSpeed = currentSpeed; // Initialize EMA
        else
            _emaSpeed = (long)(currentSpeed * SmoothingFactor + _emaSpeed * (1 - SmoothingFactor));
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