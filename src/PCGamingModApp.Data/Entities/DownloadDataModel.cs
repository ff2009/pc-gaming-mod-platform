using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Data.Entities;

public class DownloadDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? FileName { get; set; }
    public double Progress { get; set; }
    public long FileSizeInBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public DownloadStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}