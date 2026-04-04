using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Core.Helpers;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class NewDownloadDialogViewModel : ConfirmDialogViewModel
{
    private readonly IAppPaths _appPaths;
    private readonly IDialogService _dialogService;
    private readonly IDownloadOrchestrator _downloadOrchestrator;

    [ObservableProperty] private string _cancelText = "No";

    [ObservableProperty] private string _confirmText = "Yes";
    [ObservableProperty] private double _dialogHeight = double.NaN;

    [ObservableProperty] private double _dialogWidth = double.NaN;

    [ObservableProperty] private string _destinationPath = string.Empty;

    [NotifyPropertyChangedFor(nameof(DownloadSize))]
    [NotifyPropertyChangedFor(nameof(DownloadStatistics))]
    [ObservableProperty] private long _downloadFileSizeBytes = 0;

    [ObservableProperty] private string _iconText = "\xe4e0";

    [ObservableProperty] private bool _isDownloadReady = false;
    [ObservableProperty] private bool _isLoadingMetadata = false;
    [ObservableProperty] private int _metadataProgress = 0;
    [ObservableProperty] private int _metadataProgressMax = 100;
    [ObservableProperty] private string _message = "Are you sure?";

    [ObservableProperty] private ObservableCollection<NewDownloadItemViewModel> _newDownloadList = new();

    [ObservableProperty] private string _newDownloadUrl = string.Empty;

    [ObservableProperty] private string _title = "Add new downloads";

    [ObservableProperty] private SizeUnit _unit = SizeUnit.MB; // in bytes per second

    /// <summary>
    /// Design-time constructor
    /// </summary>
    public NewDownloadDialogViewModel()
    {
        if (Design.IsDesignMode)
            OnDesignTimeConstructor();
    }

    public NewDownloadDialogViewModel(
        IAppPaths appPaths,
        IDialogService dialogService,
        IDownloadOrchestrator downloadOrchestrator)
    {
        _appPaths = appPaths ?? throw new ArgumentNullException(nameof(appPaths));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _downloadOrchestrator = downloadOrchestrator ?? throw new ArgumentNullException(nameof(downloadOrchestrator));

        LoadData();
    }

    public double DownloadSize
    {
        get
        {
            double downloadSize = ConversionHelper.ConvertBytesToUnit(DownloadFileSizeBytes, Unit);
            return downloadSize;
        }
    }

    public string DownloadStatistics
    {
        get
        {
            if (NewDownloadList.Count == 0)
                return "No downloads";

            double totalSize = ConversionHelper.ConvertBytesToUnit(DownloadFileSizeBytes, Unit);
            return $"{NewDownloadList.Count} files, {totalSize:0.00} {Unit}";
        }
    }

    partial void OnNewDownloadUrlChanged(string value)
    {
        _ = GetMetadataUrlsAsync(value);
    }

    private async Task GetMetadataUrlsAsync(string urls)
    {
        string[] parts = urls.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        // Filter valid URLs upfront
        Regex regex = new Regex(
            @"(https:\/\/www\.|http:\/\/www\.|https:\/\/|http:\/\/)?[a-zA-Z0-9]{2,}(\.[a-zA-Z0-9]{2,})(\.[a-zA-Z0-9]{2,})?");
        var validUrls = parts.Where(url => regex.IsMatch(url)).Distinct().ToArray();
        if (validUrls.Length == 0) return;

        // Show progress bar and reset progress
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Busy = true;
            IsLoadingMetadata = true;
            MetadataProgress = 0;
            MetadataProgressMax = validUrls.Length;
            NewDownloadList.Clear();
        });

        // Fetch metadata asynchronously and add to list as they complete
        var tasks = validUrls.Select<string, Task<object>>(async (url, index) =>
        {
            try
            {
                var metadata = await _downloadOrchestrator.GetDownloadMetadataAsync(url);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var newItem = new NewDownloadItemViewModel()
                    {
                        Id = metadata.Id,
                        Url = metadata.Url,
                        FileName = metadata.FileName,
                        SavePath = Path.Combine(DestinationPath, metadata.FileName),
                        FileSizeInBytes = metadata.FileSizeInBytes,
                        CreatedAt = DateTime.Now
                    };

                    NewDownloadList.Add(newItem);

                    MetadataProgress = NewDownloadList.Count;
                    DownloadFileSizeBytes = NewDownloadList.Sum(x => x.FileSizeInBytes);
                });

                return metadata;
            }
            catch (Exception ex)
            {
                // Handle error for individual URL - create a failed download item
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var failedItem = new NewDownloadItemViewModel()
                    {
                        Id = Guid.NewGuid(),
                        Url = url,
                        FileName = Path.GetFileName(new Uri(url).LocalPath),
                        SavePath = Path.Combine(DestinationPath, Path.GetFileName(new Uri(url).LocalPath)),
                        FileSizeInBytes = 0,
                        CreatedAt = DateTime.Now
                    };

                    NewDownloadList.Add(failedItem);
                    MetadataProgress = NewDownloadList.Count;
                    DownloadFileSizeBytes = NewDownloadList.Sum(x => x.FileSizeInBytes);
                });
                return Task.CompletedTask;
            }
        });

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);

        // Hide progress bar when done
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsLoadingMetadata = false;
            IsDownloadReady = NewDownloadList.Count == validUrls.Length;
            Busy = false;
        });
    }

    private void OnDesignTimeConstructor()
    {
        DestinationPath = "\\Downloads";
        var downloadListMock = new List<NewDownloadItemViewModel>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                FileName = "OptiScaler_0.7.9.7z",
                SavePath = "\\Downloads\\OptiScaler_0.7.9.7z",
                FileSizeInBytes = 123456789,
                CreatedAt = DateTime.Now
            },
            new()
            {
                FileName = "Optiscaler_0.9.0-pre5 (20251031).7z",
                SavePath = "\\Downloads\\Optiscaler_0.9.0-pre5 (20251031).7z",
                FileSizeInBytes = 324535560,
                CreatedAt = DateTime.Now
            },
            new()
            {
                FileName = "dlssg-to-fsr3-0.130-738-0-130-1742150748.zip",
                SavePath = "\\Downloads\\dlssg-to-fsr3-0.130-738-0-130-1742150748.zip",
                FileSizeInBytes = 23452352,
                CreatedAt = DateTime.Now
            },
            new()
            {
                FileName = "FidelityFX-SDK-v1.1.4.zip",
                SavePath = "\\Downloads\\FidelityFX-SDK-v1.1.4.zip",
                FileSizeInBytes = 223413453,
                CreatedAt = DateTime.Now
            },
        };

        NewDownloadList = new ObservableCollection<NewDownloadItemViewModel>(downloadListMock);
    }


    private void LoadData()
    {
        DestinationPath = _appPaths.Downloads;
    }


    [RelayCommand]
    private async Task SelectDownloadDestinationPathAsync()
    {
        if (string.IsNullOrWhiteSpace(DestinationPath) || !Path.Exists(DestinationPath))
            return;

        // string? previousDestinationPath = DestinationPath;
        FolderPickerOpenOptions options = new() { Title = "Select download path", AllowMultiple = false, };

        var tempFolder = await _dialogService.FolderPickerAsync(options);
        if (!string.IsNullOrWhiteSpace(tempFolder) && Path.Exists(tempFolder))
            DestinationPath = tempFolder;

        // Update download paths
        foreach (var download in NewDownloadList)
        {
            download.SavePath = Path.Combine(DestinationPath, download.FileName);
        }
    }

    [RelayCommand]
    private void DeleteDownload(Guid id)
    {
        if (NewDownloadList.FirstOrDefault(x => x.Id == id) is { } download)
            NewDownloadList.Remove(download);
    }

    [RelayCommand(CanExecute = nameof(NotBusy))]
    private async Task StartDownloadLaterAsync()
    {
        await SaveDownloads();

        await ConfirmAsync();
    }

    [RelayCommand(CanExecute = nameof(NotBusy))]
    private async Task StartNewDownload()
    {
        await SaveDownloads();

        NewDownloadUrl = string.Empty;
        NewDownloadList.Clear();

        await ConfirmAsync();
    }

    private async Task<List<Guid>> SaveDownloads()
    {
        List<Guid> downloadIds = new();
        foreach (var download in NewDownloadList)
        {
            await _downloadOrchestrator.AddDownloadAsync(download.ToDataModel());
        }

        return downloadIds;
    }

    protected override void Cancel()
    {
        NewDownloadUrl = string.Empty;
        NewDownloadList.Clear();
        
        base.Cancel();
    }
}