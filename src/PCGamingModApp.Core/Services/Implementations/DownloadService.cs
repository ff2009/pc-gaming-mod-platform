using System.Diagnostics;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadService(
    IAppPaths appPaths,
    IHttpClientFactory httpClientFactory,
    IDownloadRepository downloadRepository,
    DownloadManager downloadManager,
    int maxParallelDownloads = 3)
    : IDownloadService
{
    private readonly SemaphoreSlim _downloadSemaphore = new(maxParallelDownloads);

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

    public async Task<DownloadDataModel> CreateDownloadAsync(string url, string savePath, int parts = 1,
        long speedLimit = 0)
    {
        var download = await GetDownloadMetadataAsync(url);
        download.SavePath = savePath;
        download.Parts = parts;
        download.CreatedAt = DateTime.Now;

        await downloadRepository.AddDownload(download);
        return download;
    }

    public async Task StartDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download != null &&
            download.Status is DownloadStatus.Pending or DownloadStatus.Paused or DownloadStatus.Failed)
        {
            downloadManager.StartTracking(download);
            _ = DownloadFileAsync(download); // Start download in background
        }

        throw new InvalidOperationException("Download cannot be started.");
    }

    public async Task PauseDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download is { Status: DownloadStatus.InProgress })
        {
            download.IsPaused = true;
            downloadManager.StopTracking(id);
            await downloadRepository.UpdateDownload(download);
        }
    }

    public async Task ResumeDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        // if (download is { IsPaused: true })
        if (download is not null)
        {
            download.IsPaused = false;
            downloadManager.StartTracking(download);
            await downloadRepository.UpdateDownload(download);
            _ = DownloadFileAsync(download); // Restart download
        }
    }

    public async Task CancelDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download != null)
        {
            downloadManager.StopTracking(id);
            download.Status = DownloadStatus.Canceled;
            await downloadRepository.UpdateDownload(download);
            DownloadProgressUpdated?.Invoke(download);
        }
    }

    public async Task DeleteDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download != null)
        { 
            downloadManager.StopTracking(id);
            await downloadRepository.DeleteDownload(id);
        }
    }
    
    public TimeSpan GetRemainingTime(Guid id)
    {
        return downloadManager.GetRemainingTime(id);
    }

    public async Task<IEnumerable<DownloadDataModel>> GetDownloadsAsync()
    {
        return await downloadRepository.GetAllDownloads();
    }

    public event Action<DownloadDataModel>? DownloadProgressUpdated;
    public event Action<DownloadDataModel>? DownloadCompleted;

    private async Task DownloadFileAsync(DownloadDataModel download)
    {
        await _downloadSemaphore.WaitAsync();
        try
        {
            using var httpClient = httpClientFactory.CreateClient();

            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(download.SavePath)!);

            // Open a stream to the file (append if resuming)
            using var fileStream = new FileStream(
                download.SavePath,
                download.DownloadedBytes > 0 ? FileMode.Append : FileMode.Create,
                FileAccess.Write);

            // Send a HEAD request to get file size (if not already known)
            if (download.FileSizeInBytes == 0)
            {
                var headResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, download.Url));
                download.FileSizeInBytes = headResponse.Content.Headers.ContentLength ?? 0;
                await downloadRepository.UpdateDownload(download);
            }

            // Create a GET request with Range header for resuming
            var request = new HttpRequestMessage(HttpMethod.Get, download.Url);
            if (download.DownloadedBytes > 0)
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(download.DownloadedBytes, null);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            int bufferSize = 32 * 1024;
            var buffer = new byte[bufferSize]; // 8KB buffer
            int bytesRead;
            var stopwatch = Stopwatch.StartNew();
            long bytesDownloadedThisSecond = 0;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                if (download.IsPaused)
                {
                    await downloadRepository.UpdateDownload(download);
                    return; // Pause: exit and release semaphore
                }

                // Write to file
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                download.DownloadedBytes += bytesRead;
                bytesDownloadedThisSecond += bytesRead;

                // Throttle speed if needed
                if (download.SpeedLimitBytesPerSecond > 0)
                {
                    var elapsed = stopwatch.ElapsedMilliseconds / 1000.0;
                    if (elapsed > 0 && bytesDownloadedThisSecond > download.SpeedLimitBytesPerSecond * elapsed)
                        await Task.Delay(100); // Simple throttling
                }

                // Reset stopwatch every second for speed calculation
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    download.DownloadSpeedInBytes = bytesDownloadedThisSecond;
                    downloadManager.UpdateProgress(download.Id, download.DownloadedBytes, bytesDownloadedThisSecond);
                    stopwatch.Restart();
                    bytesDownloadedThisSecond = 0;
                    DownloadProgressUpdated?.Invoke(download);
                }

                // Update DB and notify UI
                download.Status = DownloadStatus.InProgress;
                // Slows down the download significantly
                //await downloadRepository.UpdateDownload(download);
                DownloadProgressUpdated?.Invoke(download);
            }

            // Download complete
            download.Status = DownloadStatus.Completed;
            download.CompletedAt = DateTime.Now;
            await downloadRepository.UpdateDownload(download);
            DownloadCompleted?.Invoke(download);
        }
        catch (Exception ex)
        {
            download.Status = DownloadStatus.Failed;
            await downloadRepository.UpdateDownload(download);
            DownloadProgressUpdated?.Invoke(download);
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }
}