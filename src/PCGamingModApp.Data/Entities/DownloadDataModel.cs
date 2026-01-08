using System.ComponentModel.DataAnnotations;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Data.Entities;

public class DownloadDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [MaxLength(4096)]
    public string SavePath { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public DownloadStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public int Parts { get; set; } = 1; // For multi-part downloads
}