using System.Diagnostics;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

public class DownloadService(
    HttpClient httpClient,
    IDownloadRepository downloadRepository,
    int maxParallelDownloads = 3)
    : IDownloadService
{
    private readonly SemaphoreSlim _downloadSemaphore = new(maxParallelDownloads);

    public async Task<DownloadDataModel> GetDownloadMetadataAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        var fileName = Path.GetFileName(new Uri(url).LocalPath);

        return new DownloadDataModel
        {
            Url = url,
            FileName = fileName,
            FileSizeInBytes = contentLength
        };
    }
    
    public async Task<Guid> StartDownloadAsync(string url, string savePath, int parts = 1, long speedLimit = 0)
    {
        var download = new DownloadDataModel
        {
            Id = Guid.NewGuid(),
            Url = url,
            SavePath = savePath,
            FileName = Path.GetFileName(savePath),
            Parts = parts,
            SpeedLimitBytesPerSecond = speedLimit,
            Status = DownloadStatus.Pending,
            CreatedAt = DateTime.Now
        };

        await downloadRepository.AddDownload(download);
        return download.Id;
    }

    public async Task PauseDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download is { Status: DownloadStatus.InProgress })
        {
            download.IsPaused = true;
            await downloadRepository.UpdateDownload(download);
        }
    }

    public async Task ResumeDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download is { IsPaused: true })
        {
            download.IsPaused = false;
            await downloadRepository.UpdateDownload(download);
            _ = DownloadFileAsync(download); // Restart download
        }
    }

    public async Task CancelDownloadAsync(Guid id)
    {
        var download = await downloadRepository.GetDownloadById(id);
        if (download != null)
        {
            download.Status = DownloadStatus.Canceled;
            await downloadRepository.UpdateDownload(download);
            DownloadProgressUpdated?.Invoke(download);
        }
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
            var buffer = new byte[8192]; // 8KB buffer
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
                    stopwatch.Restart();
                    bytesDownloadedThisSecond = 0;
                }

                // Update DB and notify UI
                download.Status = DownloadStatus.InProgress;
                await downloadRepository.UpdateDownload(download);
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