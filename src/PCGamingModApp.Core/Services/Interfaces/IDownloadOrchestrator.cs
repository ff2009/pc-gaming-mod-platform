using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadOrchestrator
{
    Task<List<DownloadDataModel>> GetAllDownloadsMetadataAsync(string[] urls);
    Task<DownloadDataModel> GetDownloadMetadataAsync(string url);
    Task<List<DownloadDataModel>> GetDownloadsAsync();
    Task<DownloadDataModel?> GetDownloadByIdAsync(Guid downloadId);
    ISingleDownloadService? GetDownloadServiceById(Guid downloadId);

    IReadOnlyList<ISingleDownloadService> GetDownloadServices();
    IReadOnlyList<ISingleDownloadService> GetActiveDownloads();
    IReadOnlyList<ISingleDownloadService> GetDownloadServicesByStatus(DownloadStatus status);


    Task<DownloadDataModel> CreateDownloadAsync(string url, string savePath);

    void EnforceConcurrencyLimitAsync(int maxConcurrentDownloads);
    void EnforceSpeedLimitAsync(long maxSpeedBytesPerSecond);
    Task<DownloadStatistics> GetSystemStatisticsAsync();
    ISingleDownloadService StartTracking(DownloadDataModel download);
    void StopTracking(Guid downloadId);

    long GetCurrentSpeedLimit();
    void ReportBandwidthUsage(long speed);
}

public struct DownloadStatistics
{
    public int TotalDownloads;
    public long CurrentDownloadSpeedInBytes;
    public long PeakDownloadSpeedInBytes;
    public long SessionTrafficInBytes;
    public long TotalTrafficInBytes;
}