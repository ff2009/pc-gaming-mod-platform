using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadService
{
    Task<List<DownloadDataModel>> GetDownloadsAsync();
    Task<DownloadDataModel> GetDownloadMetadataAsync(string url);
    Task<DownloadDataModel> CreateDownloadAsync(string url, string savePath);
    Task StartDownloadAsync(Guid id);
    Task PauseDownloadAsync(Guid id);
    Task ResumeDownloadAsync(Guid id);
    Task CancelDownloadAsync(Guid id);
    Task DeleteDownloadAsync(Guid id);
    TimeSpan GetRemainingTime(Guid id);
}