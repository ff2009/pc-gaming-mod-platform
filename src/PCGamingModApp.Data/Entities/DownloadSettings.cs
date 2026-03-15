using System.ComponentModel.DataAnnotations;

namespace PCGamingModApp.Data.Entities;

public class DownloadSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(4096)]
    public string DefaultDownloadPath { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    // Bandwidth settings
    public long MaxBandwidthBytesPerSecond { get; set; } = 0; // 0 = unlimited
    public bool EnableAdaptiveBandwidth { get; set; } = true;

    // Concurrency settings  
    public int MaxConcurrentDownloads { get; set; } = 3;
    public int MaxPartsPerDownload { get; set; } = 4;

    // Retry and error handling
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 30;

    // Performance settings
    public bool EnableDownloadThrottling { get; set; } = false;
    public int ProgressUpdateIntervalMs { get; set; } = 500;

    // Queue management
    public bool AutoStartDownloads { get; set; } = true;
    public bool PauseWhenBatteryLow { get; set; } = true;
}