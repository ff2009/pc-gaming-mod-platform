using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Models;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadManager
{
    public ICollection<Guid> GetActiveDownloads();

    /// <summary>
    /// Starts tracking a new download.
    /// </summary>
    /// <param name="download">Download to track.</param>
    public void StartTracking(DownloadDataModel download);

    /// <summary>
    /// Updates the progress for a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    /// <param name="downloadedBytes">Current downloaded bytes.</param>
    /// <param name="currentSpeed">Current download speed in bytes/second.</param>
    public void UpdateProgress(Guid downloadId, long downloadedBytes, double currentSpeed);

    /// <summary>
    /// Gets the remaining time for a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    /// <returns>TimeSpan representing remaining time.</returns>
    public TimeSpan GetRemainingTime(Guid downloadId);

    /// <summary>
    /// Stops tracking a download.
    /// </summary>
    /// <param name="downloadId">ID of the download.</param>
    public void StopTracking(Guid downloadId);
}