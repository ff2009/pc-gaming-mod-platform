using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface ISingleDownloadService
{
    Guid Id { get; }
    string FileName { get; }
    string Url { get; }
    string SavePath { get; }
    long FileSizeInBytes { get; }
    long DownloadedBytes { get; }
    DownloadStatus Status { get; }
    DateTime CreatedAt { get; }
    
    long CurrentSpeedBytesPerSecond { get; }
    void UpdateSpeedLimit(long newLimit);
    
    Task StartDownloadAsync();
    void PauseDownload();
    Task ResumeDownloadAsync();
    Task CancelDownloadAsync();
    Task DeleteDownloadAsync();
    TimeSpan GetRemainingTime();
    void ValidateDownloadMetadataAsync();
    Task SplitDownloadIntoPartsAsync(int parts);
}