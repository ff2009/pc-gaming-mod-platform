using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Models;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class DownloadManager(
    IAppPaths appPaths,
    IHttpClientFactory httpClientFactory,
    IDownloadRepository downloadRepository, 
    IMessenger messenger) : IDownloadManager
{
    private readonly ConcurrentDictionary<Guid, DownloadProgressTracker> _activeDownloads = new();
    
    public async Task<DownloadDataModel> GetDownloadMetadataAsync(string url)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        var fileName = Path.GetFileName(new Uri(url).LocalPath);
        var savePath = Path.Combine(appPaths.Downloads, fileName);

        return new DownloadDataModel
        {
            Url = url,
            FileName = fileName,
            SavePath = savePath,
            FileSizeInBytes = contentLength,
            Status = DownloadStatus.Pending,
            CreatedAt = DateTime.Now
        };
    }
    
    public async Task<List<DownloadDataModel>> GetAllDownloadsMetadataAsync(string[] urls)
    {
        var tasks = urls.Select(GetDownloadMetadataAsync);
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<DownloadDataModel> CreateDownloadAsync(string url, string savePath)
    {
        var download = await GetDownloadMetadataAsync(url);
        download.SavePath = savePath;
        download.CreatedAt = DateTime.Now;

        await downloadRepository.AddDownload(download);

        // Notify the UI after adding
        messenger.Send(new DownloadAddedMessage(download));

        return download;
    }

    public Task<List<DownloadDataModel>> GetDownloadsAsync()
    {
        return downloadRepository.GetAllDownloads();
    }
    
    public ICollection<Guid> GetActiveDownloads()
    {
        return _activeDownloads.Keys;
    }

    public void StartTracking(DownloadDataModel download)
    {
        if (!_activeDownloads.TryAdd(download.Id, new DownloadProgressTracker(download)))
            return;
    }

    public void UpdateProgress(Guid downloadId, long downloadedBytes, double currentSpeed)
    {
        if (!_activeDownloads.TryGetValue(downloadId, out var tracker))
            return;

        tracker.UpdateProgress(downloadedBytes, currentSpeed);
        var download = tracker.GetDownload();
        messenger.Send(new DownloadUpdatedMessage(download));
    }

    public TimeSpan GetRemainingTime(Guid downloadId)
    {
        return _activeDownloads.TryGetValue(downloadId, out var tracker)
            ? tracker.EstimateRemainingTime()
            : TimeSpan.Zero;
    }

    public void StopTracking(Guid downloadId)
    {
        if (_activeDownloads.TryRemove(downloadId, out _))
        {
        }
    }
}