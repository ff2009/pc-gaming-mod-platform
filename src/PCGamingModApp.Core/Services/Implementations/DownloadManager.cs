using PCGamingModApp.Core.Models;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadManager
{
    private readonly Dictionary<Guid, DownloadProgressTracker> _activeDownloads = new();

    /// <summary>
    /// Starts tracking a new download.
    /// </summary>
    /// <param name="download">Download to track.</param>
    public void StartTracking(DownloadDataModel download)
    {
        if (!_activeDownloads.ContainsKey(download.Id))
        {
            _activeDownloads[download.Id] = new DownloadProgressTracker(download);
        }
    }

    /// <summary>
    /// Updates the progress for a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    /// <param name="downloadedBytes">Current downloaded bytes.</param>
    /// <param name="currentSpeed">Current download speed in bytes/second.</param>
    public void UpdateProgress(Guid downloadId, long downloadedBytes, long currentSpeed)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var tracker))
        {
            tracker.UpdateProgress(downloadedBytes, currentSpeed);
        }
    }

    /// <summary>
    /// Gets the remaining time for a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    /// <returns>TimeSpan representing remaining time.</returns>
    public TimeSpan GetRemainingTime(Guid downloadId)
    {
        if (!_activeDownloads.TryGetValue(downloadId, out var tracker))
            return TimeSpan.Zero;

        return tracker.EstimateRemainingTime();
    }

    /// <summary>
    /// Stops tracking a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    public void StopTracking(Guid downloadId)
    {
        _activeDownloads.Remove(downloadId);
    }
}