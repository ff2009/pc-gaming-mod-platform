using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadItemViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;
    
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _fileName = string.Empty;

    private long _fileSizeInBytes;
    private long _downloadedBytes;
    [ObservableProperty] private DownloadStatus _status;
    private DateTime _createdAt;
    private double _downloadSpeed; // in bytes per second
    [ObservableProperty] private string _unit = string.Empty; // in bytes per second
    private TimeSpan _eta = TimeSpan.MaxValue;

    public long FileSize => _fileSizeInBytes / 1024 / 1024;

    public long DownloadedSize => _downloadedBytes / 1024 / 1024;

    public double DownloadSpeed
    {
        get
        {
            double speed;
            switch (Unit)
            {
                case "KB":
                    speed = _downloadSpeed / 1024d;
                    break;
                case "MB":
                    speed = _downloadSpeed / (1024d * 1024d);
                    break;
                case "GB":
                    speed = _downloadSpeed / (1024d * 1024d * 1024d);
                    break;
                default:
                    speed = _downloadSpeed;
                    break;
            }
            return speed;
        }
    }

    public double Progress =>
        Math.Clamp(Math.Round(_downloadedBytes * 100d / _fileSizeInBytes, 2), 0, 100); // as percentage

    public string ETA
    {
        get
        {
            if (_eta.TotalDays >= 1) return $"{(int)_eta.TotalDays}d {_eta.Hours}h {_eta.Minutes}m";
            if (_eta.TotalHours >= 1) return $"{(int)_eta.TotalHours}h {_eta.Minutes}m {_eta.Seconds}s";
            if (_eta.TotalMinutes >= 1) return $"{(int)_eta.TotalMinutes}m {_eta.Seconds}s";
            if (_eta.TotalSeconds >= 0) return $"{(int)_eta.TotalSeconds}s";
            return "∞";
        }
    }

    [ObservableProperty] private bool _isPaused = true;

    public DownloadItemViewModel()
    {
        if (Design.IsDesignMode)
        {
            OnDesignTimeConstructor();
        }
    }
    
    public DownloadItemViewModel (IDownloadService downloadService, DownloadDataModel domain)
    {
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
        ArgumentNullException.ThrowIfNull(domain);

        Id = domain.Id;
        FileName = domain.FileName;
        // Map other properties as needed
        _fileSizeInBytes = domain.FileSizeInBytes;
        _downloadedBytes = domain.DownloadedBytes;
        _status = domain.Status;
        _createdAt = domain.CreatedAt;
    }
    
    private void OnDesignTimeConstructor()
    {
        FileName = "FidelityFX-SDK-v1.1.4.zip";
        _fileSizeInBytes = 52428800; // 50 MB
        _downloadedBytes = 34078720; // ~32.5 MB
        _status = DownloadStatus.InProgress;
        _createdAt = DateTime.Now.AddMinutes(-5);
        _downloadSpeed = 1048576; // 1 MB/s
        Unit = "MB";
        _eta = TimeSpan.FromMinutes(3666);
        IsPaused = false;
    }
    
    [RelayCommand]
    private async Task ResumeDownloadAsync(DownloadItemViewModel item)
    {
        await _downloadService.ResumeDownloadAsync(item.Id);
        IsPaused = false;
    }
    
    [RelayCommand]
    private async Task PauseDownloadAsync(DownloadItemViewModel item)
    {
        await _downloadService.ResumeDownloadAsync(item.Id);
        IsPaused = true;
    }
    
    [RelayCommand]
    private async Task CancelDownloadAsync(DownloadItemViewModel item)
    {
        await _downloadService.CancelDownloadAsync(item.Id);
        IsPaused = true;
    }
}

public static class DownloadItemViewModelExtensions
{
    /// <summary>
    /// Map the Game View Model to the Data Model (useful for persistence)
    /// </summary>
    /// <returns></returns>
    ///
    extension(DownloadItemViewModel viewModel)
    {
        public DownloadDataModel ToDataModel() => new()
        {
            Id = viewModel.Id,
            FileName = viewModel.FileName,
            Status = viewModel.Status,
        };
    }
}