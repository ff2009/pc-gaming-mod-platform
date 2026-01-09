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
    private readonly ISingleDownloadService _singleDownloadService;
    private readonly IMessenger _messenger;

    [ObservableProperty] private Guid _id;

    [ObservableProperty] private string _url;

    [ObservableProperty] private string _fileName;

    [ObservableProperty] private string _savePath;

    [NotifyPropertyChangedFor(nameof(DownloadedSize))] 
    [NotifyPropertyChangedFor(nameof(Progress))] 
    [ObservableProperty]
    private long _downloadedBytes;
    
    [NotifyPropertyChangedFor(nameof(FileSize))] 
    [ObservableProperty]
    private long _fileSizeInBytes;

    [NotifyPropertyChangedFor(nameof(InProgress))] 
    [NotifyPropertyChangedFor(nameof(IsResumeButtonVisible))] 
    [NotifyPropertyChangedFor(nameof(IsPauseButtonVisible))] 
    [ObservableProperty] private DownloadStatus _status;

    [ObservableProperty] private DateTime _createdAt;
    
    [NotifyPropertyChangedFor(nameof(DownloadSpeed))] 
    [ObservableProperty]
    private long _downloadSpeedInBytes; // in bytes per second

    [NotifyPropertyChangedFor(nameof(ETA))] 
    [ObservableProperty]
    private TimeSpan _eta = TimeSpan.MaxValue;
    
    [ObservableProperty] private SizeUnit _unit = SizeUnit.MB; // in bytes per second

    public bool InProgress => Status == DownloadStatus.InProgress;
    
    public bool IsResumeButtonVisible => Status != DownloadStatus.InProgress && Status != DownloadStatus.Completed;
    public bool IsPauseButtonVisible => Status == DownloadStatus.InProgress;

    public DownloadItemViewModel()
    {
        if (Design.IsDesignMode)
        {
            OnDesignTimeConstructor();
        }
    }

    public DownloadItemViewModel(ISingleDownloadService singleDownloadService, IMessenger messenger)
    {
        _singleDownloadService = singleDownloadService ?? throw new ArgumentNullException(nameof(singleDownloadService));
        if (!Design.IsDesignMode)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _messenger?.Register(this);
        }

        LoadData();
    }

    public long FileSize => Helpers.ConversionHelper.ConvertBytesToUnit(FileSizeInBytes, Unit);

    public long DownloadedSize => Helpers.ConversionHelper.ConvertBytesToUnit(DownloadedBytes, Unit);

    public double DownloadSpeed => Helpers.ConversionHelper.ConvertBytesToUnit(DownloadSpeedInBytes, Unit);

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
            DownloadSpeedInBytes = message.CurrentSpeedBytesPerSecond; // From message, not DB
            DownloadedBytes = _singleDownloadService.DownloadedBytes;
            Eta = _singleDownloadService.GetRemainingTime();
            Status = _singleDownloadService.Status;
        }
    }

    private void OnDesignTimeConstructor()
    {
        //FileName = "FidelityFX-SDK-v1.1.4.zip";
        FileSizeInBytes = 52428800; // 50 MB
        DownloadedBytes = 34078720; // ~32.5 MB
        Status = DownloadStatus.InProgress;
        //CreatedAt = DateTime.Now.AddMinutes(-5);
        DownloadSpeedInBytes = 1048576; // 1 MB/s
        //Unit = "MB";
        Eta = TimeSpan.FromMinutes(3666);
    }

    private void LoadData()
    {
        Id = _singleDownloadService.Id;
        Url = _singleDownloadService.Url;
        FileName = _singleDownloadService.FileName;
        SavePath = _singleDownloadService.SavePath;
        FileSizeInBytes = _singleDownloadService.FileSizeInBytes;
        DownloadedBytes = _singleDownloadService.DownloadedBytes;
        Status = _singleDownloadService.Status;
        CreatedAt = _singleDownloadService.CreatedAt;
    }

    [RelayCommand]
    private async Task ResumeDownloadAsync()
    {
        await _singleDownloadService.ResumeDownloadAsync();
        Status = _singleDownloadService.Status;
    }

    [RelayCommand]
    private void PauseDownload()
    {
        _singleDownloadService.PauseDownload();
        Status = _singleDownloadService.Status;
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
        await _singleDownloadService.DeleteDownloadAsync();
        _messenger.Send(new DownloadDeletedMessage(Id));
    }
}

public static class DownloadItemViewModelExtensions
{
    /// <summary>
    /// Map the Game View Model to the Data Model (useful for persistence)
    /// </summary>
    /// <returns></returns>
    extension(DownloadItemViewModel viewModel)
    {
        public DownloadDataModel ToDataModel() => new()
        {
            Id = viewModel.Id,
            FileName = viewModel.FileName,
            Url = viewModel.Url,
            SavePath = viewModel.SavePath,
            DownloadedBytes = viewModel.DownloadedBytes,
            FileSizeInBytes = viewModel.FileSizeInBytes,
            Status = viewModel.Status,
            CreatedAt = viewModel.CreatedAt,
        };
    }
}