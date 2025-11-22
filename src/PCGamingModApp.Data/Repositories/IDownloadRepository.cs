using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data.Repositories;

public interface IDownloadRepository
{
    Task AddDownload(DownloadDataModel download);
    Task UpdateDownload(DownloadDataModel download);
    Task DeleteDownload(Guid id);
    Task<List<DownloadDataModel>> GetAllDownloads();
    Task<DownloadDataModel?> GetDownloadById(Guid id);
}