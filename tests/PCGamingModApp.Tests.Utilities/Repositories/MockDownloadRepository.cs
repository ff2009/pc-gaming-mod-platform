using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Tests.Utilities.Repositories;

public class MockDownloadRepository : IDownloadRepository
{
    public Task<List<DownloadDataModel>> GetAllDownloads()
    {
        return Task.FromResult(new List<DownloadDataModel>());
    }

    public Task AddDownload(DownloadDataModel download)
    {
        return Task.CompletedTask;
    }

    public Task UpdateDownload(DownloadDataModel download)
    {
        return Task.CompletedTask;
    }

    public Task DeleteDownload(Guid id)
    {
        return Task.CompletedTask;
    }

    public Task<DownloadDataModel?> GetDownloadById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<DownloadDataModel> GetByIdAsync(Guid id)
    {
        return Task.FromResult(new DownloadDataModel { Id = id });
    }
}