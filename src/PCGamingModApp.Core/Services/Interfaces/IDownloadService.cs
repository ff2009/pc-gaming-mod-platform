using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadService
{
    Task<IEnumerable<DownloadDataModel>> GetDownloadsAsync();
    Task<DownloadDataModel> GetDownloadMetadataAsync(string url);
    Task<DownloadDataModel> CreateDownloadAsync(string url, string savePath, int parts = 1, long speedLimit = 0);
    Task StartDownloadAsync(Guid id);
    Task PauseDownloadAsync(Guid id);
    Task ResumeDownloadAsync(Guid id);
    Task CancelDownloadAsync(Guid id);
    Task DeleteDownloadAsync(Guid id);
    TimeSpan GetRemainingTime(Guid id);
    event Action<DownloadDataModel> DownloadProgressUpdated;
    event Action<DownloadDataModel> DownloadCompleted;
}