using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Messaging.Messages;

public class DownloadAddedMessage(DownloadDataModel download)
{
    public DownloadDataModel Download { get; } = download;
}

public class DownloadUpdatedMessage(DownloadDataModel download)
{
    public DownloadDataModel Download { get; } = download;
}

public class DownloadDeletedMessage(Guid downloadId)
{
    public Guid DownloadId { get; } = downloadId;
}