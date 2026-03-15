using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class DownloadOrchestrator : IDownloadOrchestrator
{
    private readonly IAppPaths _appPaths;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDownloadRepository _downloadRepository;
    private readonly IDownloadServiceFactory _downloadServiceFactory;
    private readonly IMessenger _messenger;
    private readonly IDownloadSettingsService _settingsService;

    /// <summary>
    /// Store all download services
    /// </summary>
    private readonly ConcurrentDictionary<Guid, ISingleDownloadService> _allDownloads = new();

    private long _globalSpeedLimitBytesPerSecond = 0; // Default: unlimited
    private long _currentTotalBandwidthUsed = 0;
    private readonly Lock _bandwidthLock = new();

    public DownloadOrchestrator(IAppPaths appPaths,
        IHttpClientFactory httpClientFactory,
        IDownloadRepository downloadRepository,
        IDownloadServiceFactory downloadServiceFactory,
        IMessenger messenger,
        IDownloadSettingsService settingsService)
    {
        _appPaths = appPaths;
        _httpClientFactory = httpClientFactory;
        _downloadRepository = downloadRepository;
        _downloadServiceFactory = downloadServiceFactory;
        _messenger = messenger;
        _settingsService = settingsService;
        
        // Register for settings changes
        _messenger.Register<DownloadSettingsChangedMessage>(this, (recipient, message) => 
            ApplySettings(message.Settings));
    }

    public async Task InitializeDownloadOrchestrator()
    {
        // Load existing downloads from repository and start tracking them
        var downloads = await _downloadRepository.GetAllDownloads();
        foreach (var download in downloads)
        {
            StartTracking(download);
        }
    }

    public async Task<DownloadDataModel> CreateDownloadAsync(string url, string savePath)
    {
        var download = await GetDownloadMetadataAsync(url);
        download.SavePath = savePath;
        download.CreatedAt = DateTime.Now;

        await _downloadRepository.AddDownload(download);

        // Start tracking
        var service = StartTracking(download);

        // Send message with both download and service
        _messenger.Send(new DownloadAddedMessage(download, service));

        return download;
    }

    public async Task<DownloadDataModel> GetDownloadMetadataAsync(string url)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        var fileName = Path.GetFileName(new Uri(url).LocalPath);
        var savePath = Path.Combine(_appPaths.Downloads, fileName);

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
        return (await Task.WhenAll(tasks)).ToList();
    }

    public Task<List<DownloadDataModel>> GetDownloadsAsync()
    {
        return _downloadRepository.GetAllDownloads();
    }

    public Task<DownloadDataModel?> GetDownloadByIdAsync(Guid downloadId)
    {
        return _downloadRepository.GetDownloadById(downloadId);
    }

    public ISingleDownloadService? GetDownloadServiceById(Guid downloadId)
    {
        _allDownloads.TryGetValue(downloadId, out var service);
        return service;
    }

    public IReadOnlyList<ISingleDownloadService> GetDownloadServices()
    {
        return _allDownloads.Select(x => x.Value).ToList();
    }

    public IReadOnlyList<ISingleDownloadService> GetActiveDownloads()
    {
        return _allDownloads.Where(x => x.Value.Status == DownloadStatus.InProgress).Select(x => x.Value).ToList();
    }

    public IReadOnlyList<ISingleDownloadService> GetDownloadServicesByStatus(DownloadStatus status)
    {
        return _allDownloads.Where(x => x.Value.Status == status).Select(x => x.Value).ToList();
    }

    public void EnforceConcurrencyLimitAsync(int maxConcurrentDownloads)
    {
        // Logic to enforce concurrency (e.g., adjust SemaphoreSlim in SingleDownloadService)
    }

    public void EnforceSpeedLimitAsync(long maxSpeedBytesPerSecond)
    {
        _globalSpeedLimitBytesPerSecond = maxSpeedBytesPerSecond;
        RebalanceBandwidth();
    }

    public async Task<DownloadStatistics> GetSystemStatisticsAsync()
    {
        var downloads = await _downloadRepository.GetAllDownloads();
        return new DownloadStatistics
        {
            TotalDownloads = downloads.Count,
            TotalTrafficInBytes = downloads.Sum(d => d.DownloadedBytes)
        };
    }

    public ISingleDownloadService StartTracking(DownloadDataModel download)
    {
        if (!_allDownloads.TryGetValue(download.Id, out var service))
        {
            service = _downloadServiceFactory.Create(
                download,
                GetFairShareSpeedLimit,
                ReportBandwidthUsage);

            _allDownloads.TryAdd(download.Id, service);
        }

        return service;
    }

    public void StopTracking(Guid downloadId)
    {
        _allDownloads.TryRemove(downloadId, out _);
    }

    private long GetFairShareSpeedLimit()
    {
        if (_globalSpeedLimitBytesPerSecond <= 0) return 0; // Unlimited
        return _allDownloads.Count == 0
            ? _globalSpeedLimitBytesPerSecond
            : _globalSpeedLimitBytesPerSecond / _allDownloads.Count;
    }

    public long GetCurrentSpeedLimit() => _globalSpeedLimitBytesPerSecond;

    public void ReportBandwidthUsage(long speedBytesPerSecond)
    {
        lock (_bandwidthLock)
        {
            _currentTotalBandwidthUsed = _allDownloads.Sum(s => s.Value.CurrentSpeedBytesPerSecond);
            if (_currentTotalBandwidthUsed > _globalSpeedLimitBytesPerSecond)
                RebalanceBandwidth();
        }
    }

    private void RebalanceBandwidth()
    {
        if (_globalSpeedLimitBytesPerSecond <= 0) return;
        long fairShare = _globalSpeedLimitBytesPerSecond / Math.Max(1, _allDownloads.Count);
        foreach (var service in _allDownloads.Values)
            service.UpdateSpeedLimit(fairShare);
    }
    
    private void ApplySettings(DownloadSettings settings)
    {
        // Apply bandwidth settings
        EnforceSpeedLimitAsync(settings.MaxBandwidthBytesPerSecond);
        
        // Apply concurrency settings  
        EnforceConcurrencyLimitAsync(settings.MaxConcurrentDownloads);
        
        // Apply parts per download settings (would need to be implemented in SingleDownloadService)
        // TODO: Implement parts per download configuration
        
        // Apply adaptive bandwidth setting
        // TODO: Implement adaptive bandwidth logic
    }
}