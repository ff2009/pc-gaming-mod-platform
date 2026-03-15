using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data.Repositories;

public interface IDownloadSettingsRepository
{
    Task<DownloadSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(DownloadSettings settings);
    Task InitializeDefaultSettingsAsync();
}