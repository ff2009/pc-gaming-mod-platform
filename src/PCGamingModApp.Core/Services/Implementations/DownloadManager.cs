using PCGamingModApp.Core.Models;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadManager
{
    private readonly Dictionary<Guid, DownloadProgressTracker> _activeDownloads = new();
    
    public TimeSpan GetRemainingTime(Guid downloadId)
    {
        if (!_activeDownloads.TryGetValue(downloadId, out var tracker))
            return TimeSpan.Zero;

        return tracker.EstimateRemainingTime();
    }
}