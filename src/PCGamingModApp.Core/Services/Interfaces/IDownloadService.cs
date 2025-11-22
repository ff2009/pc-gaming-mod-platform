using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadService
{
    Task<Guid> StartDownloadAsync(string url, string savePath, int parts = 1, long speedLimit = 0);
    Task PauseDownloadAsync(Guid id);
    Task ResumeDownloadAsync(Guid id);
    Task CancelDownloadAsync(Guid id);
    Task<IEnumerable<DownloadDataModel>> GetDownloadsAsync();
    event Action<DownloadDataModel> DownloadProgressUpdated;
    event Action<DownloadDataModel> DownloadCompleted;
}