using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Messaging.Messages;

public class DownloadAddedMessage(DownloadDataModel download, ISingleDownloadService downloadService)
{
    public DownloadDataModel Download { get; } = download;
    public ISingleDownloadService DownloadService { get; } = downloadService;
}

public class DownloadUpdatedMessage(DownloadDataModel download, long currentSpeedBytesPerSecond)
{
    public DownloadDataModel Download { get; } = download;
    public long CurrentSpeedBytesPerSecond { get; } = currentSpeedBytesPerSecond;
}

public class DownloadDeletedMessage(Guid downloadId)
{
    public Guid DownloadId { get; } = downloadId;
}