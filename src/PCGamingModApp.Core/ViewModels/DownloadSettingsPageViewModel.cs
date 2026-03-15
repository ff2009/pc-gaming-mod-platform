using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadSettingsPageViewModel : PageViewModel
{
    private readonly IDownloadSettingsService _settingsService;
    private DownloadSettings _currentSettings = new();
    
    public DownloadSettingsPageViewModel(IDownloadSettingsService settingsService) : base(ApplicationPageNames.LauncherSettings)
    {
        _settingsService = settingsService;
        _ = LoadSettingsAsync();
    }
    
    public override string PageTitle => "Download Settings";
    
    [ObservableProperty]
    private string _defaultDownloadPath = string.Empty;
    
    [ObservableProperty]
    private long _maxBandwidthBytesPerSecond;
    
    [ObservableProperty]
    private bool _enableAdaptiveBandwidth;
    
    [ObservableProperty]
    private int _maxConcurrentDownloads;
    
    [ObservableProperty]
    private int _maxPartsPerDownload;
    
    [ObservableProperty]
    private int _maxRetryAttempts;
    
    [ObservableProperty]
    private int _retryDelaySeconds;
    
    [ObservableProperty]
    private bool _enableDownloadThrottling;
    
    [ObservableProperty]
    private int _progressUpdateIntervalMs;
    
    [ObservableProperty]
    private bool _autoStartDownloads;
    
    [ObservableProperty]
    private bool _pauseWhenBatteryLow;
    
    [ObservableProperty]
    private bool _isSaving;
    
    private async Task LoadSettingsAsync()
    {
        _currentSettings = await _settingsService.GetSettingsAsync();
        UpdateViewModelFromSettings();
    }
    
    private void UpdateViewModelFromSettings()
    {
        DefaultDownloadPath = _currentSettings.DefaultDownloadPath;
        MaxBandwidthBytesPerSecond = _currentSettings.MaxBandwidthBytesPerSecond;
        EnableAdaptiveBandwidth = _currentSettings.EnableAdaptiveBandwidth;
        MaxConcurrentDownloads = _currentSettings.MaxConcurrentDownloads;
        MaxPartsPerDownload = _currentSettings.MaxPartsPerDownload;
        MaxRetryAttempts = _currentSettings.MaxRetryAttempts;
        RetryDelaySeconds = _currentSettings.RetryDelaySeconds;
        EnableDownloadThrottling = _currentSettings.EnableDownloadThrottling;
        ProgressUpdateIntervalMs = _currentSettings.ProgressUpdateIntervalMs;
        AutoStartDownloads = _currentSettings.AutoStartDownloads;
        PauseWhenBatteryLow = _currentSettings.PauseWhenBatteryLow;
    }
    
    private void UpdateSettingsFromViewModel()
    {
        _currentSettings.DefaultDownloadPath = DefaultDownloadPath;
        _currentSettings.MaxBandwidthBytesPerSecond = MaxBandwidthBytesPerSecond;
        _currentSettings.EnableAdaptiveBandwidth = EnableAdaptiveBandwidth;
        _currentSettings.MaxConcurrentDownloads = MaxConcurrentDownloads;
        _currentSettings.MaxPartsPerDownload = MaxPartsPerDownload;
        _currentSettings.MaxRetryAttempts = MaxRetryAttempts;
        _currentSettings.RetryDelaySeconds = RetryDelaySeconds;
        _currentSettings.EnableDownloadThrottling = EnableDownloadThrottling;
        _currentSettings.ProgressUpdateIntervalMs = ProgressUpdateIntervalMs;
        _currentSettings.AutoStartDownloads = AutoStartDownloads;
        _currentSettings.PauseWhenBatteryLow = PauseWhenBatteryLow;
    }
    
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsSaving = true;
        
        try
        {
            UpdateSettingsFromViewModel();
            await _settingsService.UpdateSettingsAsync(_currentSettings);
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    [RelayCommand]
    private void ResetToDefaults()
    {
        _currentSettings = new DownloadSettings();
        UpdateViewModelFromSettings();
    }
}