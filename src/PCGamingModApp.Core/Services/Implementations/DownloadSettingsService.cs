using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadSettingsService(IDownloadSettingsRepository settingsRepository, IMessenger messenger)
    : IDownloadSettingsService
{
    private DownloadSettings _currentSettings = new();

    public async Task<DownloadSettings> GetSettingsAsync()
    {
        if (_currentSettings.Id == Guid.Empty)
        {
            _currentSettings = await settingsRepository.GetSettingsAsync();
        }
        return _currentSettings;
    }
    
    public async Task UpdateSettingsAsync(DownloadSettings settings)
    {
        _currentSettings = settings;
        await settingsRepository.UpdateSettingsAsync(settings);
        await ApplySettingsAsync();
    }
    
    public async Task ApplySettingsAsync()
    {
        // Broadcast settings change to all interested services
        messenger.Send(new DownloadSettingsChangedMessage(_currentSettings));
    }
    
    // Real-time accessors
    public long GetMaxBandwidth() => _currentSettings.MaxBandwidthBytesPerSecond;
    public int GetMaxConcurrentDownloads() => _currentSettings.MaxConcurrentDownloads;
    public int GetMaxPartsPerDownload() => _currentSettings.MaxPartsPerDownload;
    public bool GetAdaptiveBandwidthEnabled() => _currentSettings.EnableAdaptiveBandwidth;
}