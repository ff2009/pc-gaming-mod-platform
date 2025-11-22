using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadService(HttpClient httpClient, IDownloadRepository repository, int maxParallelDownloads = 3)
    : IDownloadService
{
    private readonly SemaphoreSlim _downloadSemaphore = new(maxParallelDownloads);

    public async Task<Guid> StartDownloadAsync(string url, string savePath, int parts = 1, long speedLimit = 0)
    {
        var download = new DownloadDataModel
            { Url = url, SavePath = savePath, Parts = parts, SpeedLimitBytesPerSecond = speedLimit };
        await repository.AddDownload(download);

        _ = DownloadFileAsync(download); // Fire-and-forget
        return download.Id;
    }

    public Task PauseDownloadAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task ResumeDownloadAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task CancelDownloadAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DownloadDataModel>> GetDownloadsAsync()
    {
        throw new NotImplementedException();
    }

    public event Action<DownloadDataModel>? DownloadProgressUpdated;
    public event Action<DownloadDataModel>? DownloadCompleted;

    private async Task DownloadFileAsync(DownloadDataModel download)
    {
        await _downloadSemaphore.WaitAsync();
        try
        {
            using var response = await httpClient.GetAsync(download.Url, HttpCompletionOption.ResponseHeadersRead);
            // Implement multi-part download, speed limiting, and progress reporting here
            // Update `download.DownloadedBytes` and `download.Status` in real-time
            // Trigger `DownloadProgressUpdated` event
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }
}