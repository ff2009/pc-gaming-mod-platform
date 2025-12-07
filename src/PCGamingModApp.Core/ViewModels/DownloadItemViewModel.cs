using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadItemViewModel : ViewModelBase, IRecipient<DownloadUpdatedMessage>
{
    private readonly IDownloadService _downloadService;
    private readonly IMessenger _messenger;

    private DateTime _createdAt;

    [NotifyPropertyChangedFor(nameof(Progress))] [NotifyPropertyChangedFor(nameof(DownloadedSize))] [ObservableProperty]
    private long _downloadedBytes;

    [NotifyPropertyChangedFor(nameof(DownloadSpeed))] [ObservableProperty]
    private double _downloadSpeedInBytes; // in bytes per second

    [NotifyPropertyChangedFor(nameof(ETA))] [ObservableProperty]
    private TimeSpan _eta = TimeSpan.MaxValue;

    [ObservableProperty] private string _fileName = string.Empty;

    [ObservableProperty] private long _fileSizeInBytes;

    [ObservableProperty] private Guid _id;

    [ObservableProperty] private bool _isPaused = true;
    [ObservableProperty] private string _savePath = string.Empty;

    [ObservableProperty] private DownloadStatus _status;

    [ObservableProperty] private TimeSpan _timeElapsed = TimeSpan.Zero;

    [ObservableProperty] private string _unit = "MB"; // in bytes per second
    [ObservableProperty] private string _url = string.Empty;

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
        if (!Design.IsDesignMode)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _messenger?.Register<DownloadUpdatedMessage>(this);
        }
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

    }

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
                    speed = DownloadSpeedInBytes / 1024d;
                    break;
                case "MB":
                    speed = DownloadSpeedInBytes / (1024d * 1024d);
                    break;
                case "GB":
                    speed = DownloadSpeedInBytes / (1024d * 1024d * 1024d);
                    break;
                default:
                    speed = DownloadSpeedInBytes;
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
            if (Eta.TotalDays > 99) return "∞";
            if (Eta.TotalDays >= 1) return $"{(int)Eta.TotalDays}d {Eta.Hours}h {Eta.Minutes}m";
            if (Eta.TotalHours >= 1) return $"{(int)Eta.TotalHours}h {Eta.Minutes}m {Eta.Seconds}s";
            if (Eta.TotalMinutes >= 1) return $"{(int)Eta.TotalMinutes}m {Eta.Seconds}s";
            if (Eta.TotalSeconds >= 0) return $"{(int)Eta.TotalSeconds}s";
            return "∞";
        }
    }

    public void Receive(DownloadUpdatedMessage message)
    {
        if (message.Download.Id == Id)
        {
            DownloadedBytes = message.Download.DownloadedBytes;
            DownloadSpeedInBytes = message.Download.DownloadSpeedInBytes;
            Status = message.Download.Status;
            Eta = _downloadService.GetRemainingTime(Id);
        }
    }

    private void OnDesignTimeConstructor()
    {
        FileName = "FidelityFX-SDK-v1.1.4.zip";
        FileSizeInBytes = 52428800; // 50 MB
        DownloadedBytes = 34078720; // ~32.5 MB
        Status = DownloadStatus.InProgress;
        _createdAt = DateTime.Now.AddMinutes(-5);
        DownloadSpeedInBytes = 1048576; // 1 MB/s
        Unit = "MB";
        Eta = TimeSpan.FromMinutes(3666);
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
        await _downloadService.PauseDownloadAsync(Id);
        IsPaused = true;
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        // TODO : Move to a platform-specific service
        var folderPath = Path.GetDirectoryName(SavePath);
        if (folderPath != null && Directory.Exists(folderPath))
        {
            _ = Process.Start(new ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
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