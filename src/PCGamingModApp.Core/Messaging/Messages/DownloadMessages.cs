using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.Messaging.Messages;

public class DownloadAddedMessage(DownloadDataModel download, ISingleDownloadService downloadService)
{
    public DownloadDataModel Download { get; } = download;
    public ISingleDownloadService DownloadService { get; } = downloadService;
}

public class DownloadUpdatedMessage(DownloadDataModel download)
{
    public DownloadDataModel Download { get; } = download;
}

public class DownloadDeletedMessage(Guid downloadId)
{
    public Guid DownloadId { get; } = downloadId;
}

public class DownloadStatisticsMessage(DownloadStatistics downloadStatistics)
{
    public DownloadStatistics DownloadStatistics { get; } = downloadStatistics;
}

public class DownloadStatusUpdatedMessage(Guid downloadId, DownloadStatus status)
{
    public Guid DownloadId { get; } = downloadId;   
    public DownloadStatus Status { get; } = status;
}

public class DownloadProgressUpdatedMessage(
    Guid downloadId,
    long currentSpeedBytesPerSecond,
    long downloadedBytes,
    TimeSpan eta)
{
    public Guid DownloadId { get; } = downloadId;
    public long CurrentSpeedBytesPerSecond { get; } = currentSpeedBytesPerSecond;
    public long DownloadedBytes { get; } = downloadedBytes;
    public TimeSpan ETA { get; } = eta;
}