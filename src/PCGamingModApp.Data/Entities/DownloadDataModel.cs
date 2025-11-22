using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Data.Entities;

public class DownloadDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SavePath { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public DownloadStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public int Parts { get; set; } = 1; // For multi-part downloads
    public long SpeedLimitBytesPerSecond { get; set; } = 0; // 0 = unlimited
    
    public long LastDownloadedBytes { get; set; } // For resuming
    public bool IsPaused { get; set; }
}