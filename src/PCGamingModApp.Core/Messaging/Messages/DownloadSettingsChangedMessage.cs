using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Messaging.Messages;

public class DownloadSettingsChangedMessage(DownloadSettings settings)
{
    public DownloadSettings Settings { get; } = settings;
}