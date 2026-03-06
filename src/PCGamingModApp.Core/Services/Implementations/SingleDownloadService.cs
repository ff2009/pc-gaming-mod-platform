using System.Diagnostics;
using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Models;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class SingleDownloadService : ISingleDownloadService
{
    private readonly DownloadDataModel _download;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDownloadRepository _downloadRepository;
    private readonly IMessenger _messenger;
    private readonly Action<long> _reportBandwidthUsage;
    
    private readonly DownloadProgressTracker _progressTracker;
    private readonly SemaphoreSlim _downloadSemaphore;
    private long _currentSpeedLimit = 0;

    public Guid Id => this._download.Id;
    public string FileName => this._download.FileName;
    public string Url => this._download.Url;
    public string SavePath => this._download.SavePath;
    public long FileSizeInBytes => this._download.FileSizeInBytes;
    public long DownloadedBytes => this._download.DownloadedBytes;
    public DownloadStatus Status => this._download.Status;
    public DateTime CreatedAt => this._download.CreatedAt;

    public long CurrentSpeedBytesPerSecond => _progressTracker.CurrentSpeedBytesPerSecond; //_currentSpeedBytesPerSecond;

    public SingleDownloadService(DownloadDataModel download,
        IHttpClientFactory httpClientFactory,
        IDownloadRepository downloadRepository,
        IMessenger messenger,
        Func<long> getSpeedLimit, // Delegate to fetch current fair share
        Action<long> reportBandwidthUsage, // Callback to report speed
        int maxParallelDownloads = 3)
    {
        _httpClientFactory = httpClientFactory;
        _downloadRepository = downloadRepository;
        _messenger = messenger;
        _reportBandwidthUsage = reportBandwidthUsage;
        _download = download;
        _progressTracker = new DownloadProgressTracker(download);
        _downloadSemaphore = new SemaphoreSlim(maxParallelDownloads);
    }

    public long CurrentSpeed { get; private set; } // Add this property

    public void UpdateSpeedLimit(long newLimit) => _currentSpeedLimit = newLimit;

    public Task StartDownloadAsync()
    {
        if (_download.Status is DownloadStatus.NotStarted or DownloadStatus.Pending or DownloadStatus.Paused
            or DownloadStatus.Failed)
        {
            _ = DownloadFileAsync();
        }

        return Task.CompletedTask;
    }

    public void PauseDownload()
    {
        if (_download.Status == DownloadStatus.InProgress)
        {
            _download.Status = DownloadStatus.Paused;
            _messenger.Send(new DownloadStatusUpdatedMessage(_download.Id, _download.Status));
        }
    }

    public Task ResumeDownloadAsync()
    {
        if (_download.Status is DownloadStatus.NotStarted or DownloadStatus.Pending or DownloadStatus.Paused
            or DownloadStatus.Failed)
        {
            return DownloadFileAsync();
        }

        return Task.CompletedTask;
    }

    public async Task CancelDownloadAsync()
    {
        _download.Status = DownloadStatus.Canceled;
        _messenger.Send(new DownloadStatusUpdatedMessage(_download.Id, _download.Status));
        await _downloadRepository.UpdateDownload(_download);
        _messenger.Send(new DownloadUpdatedMessage(_download));
    }

    public Task DeleteDownloadAsync()
    {
        return _downloadRepository.DeleteDownload(_download.Id);
    }

    public TimeSpan GetRemainingTime()
    {
        return _progressTracker.EstimateRemainingTime();
    }

    public void ValidateDownloadMetadataAsync()
    {
        if (_download.FileSizeInBytes <= 0)
            throw new InvalidOperationException("File size must be greater than zero.");
        if (string.IsNullOrWhiteSpace(_download.Url))
            throw new InvalidOperationException("URL cannot be empty.");
    }

    public async Task SplitDownloadIntoPartsAsync(int parts)
    {
        if (parts <= 0)
            throw new ArgumentException("Parts must be greater than zero.");
        _download.Parts = parts;
        await _downloadRepository.UpdateDownload(_download);
    }

    private async Task DownloadFileAsync()
    {
        await _downloadSemaphore.WaitAsync();
        const int bufferSize = 64 * 1024;

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            Directory.CreateDirectory(Path.GetDirectoryName(_download.SavePath)!);

            await using var fileStream = new FileStream(
                _download.SavePath,
                _download.DownloadedBytes > 0 ? FileMode.Append : FileMode.Create,
                FileAccess.Write);

            if (_download.FileSizeInBytes == 0)
            {
                var headResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _download.Url));
                _download.FileSizeInBytes = headResponse.Content.Headers.ContentLength ?? 0;
                await _downloadRepository.UpdateDownload(_download);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _download.Url);
            if (_download.DownloadedBytes > 0)
                request.Headers.Range = new RangeHeaderValue(_download.DownloadedBytes, null);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            var buffer = new byte[bufferSize];
            int bytesRead;
            var stopwatch = Stopwatch.StartNew();

            _download.Status = DownloadStatus.InProgress;
            _messenger.Send(new DownloadStatusUpdatedMessage(_download.Id, _download.Status));
            await _downloadRepository.UpdateDownload(_download);

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                if (_download.Status == DownloadStatus.Paused)
                {
                    await _downloadRepository.UpdateDownload(_download);
                    return;
                }

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                _download.DownloadedBytes += bytesRead;
                _progressTracker.UpdateProgress(bytesRead);

                // Apply dynamic throttling
                if (_currentSpeedLimit > 0 && _progressTracker.CurrentSpeedBytesPerSecond > _currentSpeedLimit)
                    await Task.Delay(100); // Throttle if exceeding fair share
                
                // Report speed to orchestrator
                _reportBandwidthUsage(_progressTracker.CurrentSpeedBytesPerSecond);
                
                if (stopwatch.ElapsedMilliseconds >= 100)
                {
                    //_currentSpeedBytesPerSecond = bytesDownloadedThisSecond;
                    stopwatch.Restart();
                    //_progressTracker.UpdateProgress(bytesRead, _currentSpeedBytesPerSecond);
                    _messenger.Send(new DownloadProgressUpdatedMessage(_download.Id, _progressTracker.CurrentSpeedBytesPerSecond, _download.DownloadedBytes, GetRemainingTime()));
                }

                _messenger.Send(new DownloadUpdatedMessage(_download));
            }

            _messenger.Send(new DownloadProgressUpdatedMessage(_download.Id, _progressTracker.CurrentSpeedBytesPerSecond, _download.DownloadedBytes, GetRemainingTime()));
            _download.Status = DownloadStatus.Completed;
            _messenger.Send(new DownloadStatusUpdatedMessage(_download.Id, _download.Status));
            _download.CompletedAt = DateTime.Now;
            await _downloadRepository.UpdateDownload(_download);
            _messenger.Send(new DownloadUpdatedMessage(_download));
        }
        catch (Exception ex)
        {
            _download.Status = DownloadStatus.Failed;
            _messenger.Send(new DownloadStatusUpdatedMessage(_download.Id, _download.Status));
            await _downloadRepository.UpdateDownload(_download);
            _messenger.Send(new DownloadUpdatedMessage(_download));
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }
}