using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadItemViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;
    private readonly IMessenger _messenger;

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _savePath = string.Empty;

    [ObservableProperty] private long _fileSizeInBytes;
    
    [NotifyPropertyChangedFor(nameof(Progress))] 
    [NotifyPropertyChangedFor(nameof(DownloadedSize))]
    [ObservableProperty]
    private long _downloadedBytes;
    [ObservableProperty] private DownloadStatus _status;
    private DateTime _createdAt;
    private double _downloadSpeed; // in bytes per second
    [ObservableProperty] private string _unit = string.Empty; // in bytes per second
    [ObservableProperty] private TimeSpan _timeElapsed = TimeSpan.Zero;
    private TimeSpan _eta = TimeSpan.MaxValue;

    public long FileSize => FileSizeInBytes / 1024 / 1024;

    public long DownloadedSize => DownloadedBytes / 1024 / 1024;

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
        Math.Clamp(Math.Round(DownloadedBytes * 100d / FileSizeInBytes, 2), 0, 100); // as percentage

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

    public DownloadItemViewModel(IDownloadService downloadService, IMessenger messenger, DownloadDataModel domain)
    {
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
        _messenger = messenger; // ?? throw new ArgumentNullException(nameof(messenger));
        ArgumentNullException.ThrowIfNull(domain);

        _id = domain.Id;
        _fileName = domain.FileName;
        _url = domain.Url;
        _savePath = domain.SavePath;
        // Map other properties as needed
        _fileSizeInBytes = domain.FileSizeInBytes;
        _downloadedBytes = domain.DownloadedBytes;
        _status = domain.Status;
        _createdAt = domain.CreatedAt;

        _downloadService.DownloadProgressUpdated += (download) =>
        {
            DownloadedBytes = download.DownloadedBytes;
            _downloadSpeed = download.SpeedLimitBytesPerSecond;
            Status = download.Status;
        };
    }

    private void OnDesignTimeConstructor()
    {
        FileName = "FidelityFX-SDK-v1.1.4.zip";
        FileSizeInBytes = 52428800; // 50 MB
        DownloadedBytes = 34078720; // ~32.5 MB
        Status = DownloadStatus.InProgress;
        _createdAt = DateTime.Now.AddMinutes(-5);
        _downloadSpeed = 1048576; // 1 MB/s
        Unit = "MB";
        _eta = TimeSpan.FromMinutes(3666);
        IsPaused = false;
    }

    [RelayCommand]
    private async Task ResumeDownloadAsync()
    {
        await _downloadService.ResumeDownloadAsync(Id);
        IsPaused = false;
    }

    [RelayCommand]
    private async Task PauseDownloadAsync()
    {
        await _downloadService.ResumeDownloadAsync(Id);
        IsPaused = true;
    }

    [RelayCommand]
    private async Task CancelDownloadAsync()
    {
        // await _downloadService.CancelDownloadAsync(Id);
        await _downloadService.DeleteDownloadAsync(Id);
        _messenger.Send(new DownloadDeletedMessage(Id));
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
            Url = viewModel.Url,
            SavePath = viewModel.SavePath,
            FileSizeInBytes = viewModel.FileSizeInBytes,
            Status = viewModel.Status,
        };
    }
}