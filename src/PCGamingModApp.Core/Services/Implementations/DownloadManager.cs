using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Models;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadManager
{
    private readonly ConcurrentDictionary<Guid, DownloadProgressTracker> _activeDownloads = new();
    private readonly IMessenger _messenger;

    public DownloadManager(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public ICollection<Guid> GetActiveDownloads()
    {
        return _activeDownloads.Keys;
    }

    /// <summary>
    /// Starts tracking a new download.
    /// </summary>
    /// <param name="download">Download to track.</param>
    public void StartTracking(DownloadDataModel download)
    {
        if (!_activeDownloads.TryAdd(download.Id, new DownloadProgressTracker(download)))
            return;

        //_messenger.Send(new DownloadUpdatedMessage(download));
    }

    /// <summary>
    /// Updates the progress for a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    /// <param name="downloadedBytes">Current downloaded bytes.</param>
    /// <param name="currentSpeed">Current download speed in bytes/second.</param>
    public void UpdateProgress(Guid downloadId, long downloadedBytes, double currentSpeed)
    {
        if (!_activeDownloads.TryGetValue(downloadId, out var tracker))
            return;

        tracker.UpdateProgress(downloadedBytes, currentSpeed);
        var download = tracker.GetDownload();
        _messenger.Send(new DownloadUpdatedMessage(download));
    }

    /// <summary>
    /// Gets the remaining time for a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    /// <returns>TimeSpan representing remaining time.</returns>
    public TimeSpan GetRemainingTime(Guid downloadId)
    {
        return _activeDownloads.TryGetValue(downloadId, out var tracker)
            ? tracker.EstimateRemainingTime()
            : TimeSpan.Zero;
    }

    /// <summary>
    /// Stops tracking a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    public void StopTracking(Guid downloadId)
    {
        if (_activeDownloads.TryRemove(downloadId, out _))
        {
            //_messenger.Send(new DownloadUpdatedMessage(downloadId));
        }
    }
}