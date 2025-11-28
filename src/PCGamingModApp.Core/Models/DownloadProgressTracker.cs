using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Models;

public class DownloadProgressTracker(DownloadDataModel download)
{
    private double _emaSpeed;
    private const double SmoothingFactor = 0.2;

    public void UpdateProgress(long downloadedBytes, double currentSpeed)
    {
        download.DownloadedBytes = downloadedBytes;
        _emaSpeed = currentSpeed * SmoothingFactor + _emaSpeed * (1 - SmoothingFactor);
    }

    public TimeSpan EstimateRemainingTime()
    {
        if (_emaSpeed <= 0) return TimeSpan.Zero;
        long remainingBytes = download.FileSizeInBytes - download.DownloadedBytes;
        return TimeSpan.FromSeconds(remainingBytes / _emaSpeed);
    }
}