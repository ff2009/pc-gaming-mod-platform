using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadSettingsService
{
    Task<DownloadSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(DownloadSettings settings);
    Task ApplySettingsAsync();
    
    // Real-time setting getters for other services
    long GetMaxBandwidth();
    int GetMaxConcurrentDownloads();
    int GetMaxPartsPerDownload();
    bool GetAdaptiveBandwidthEnabled();
}