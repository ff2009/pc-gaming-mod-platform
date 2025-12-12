using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Core.Helpers;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class NewDownloadDialogViewModel : DialogViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IDownloadService _downloadService;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _busy = false;

    [ObservableProperty] private string _cancelText = "No";

    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string _confirmText = "Yes";
    [ObservableProperty] private double _dialogHeight = double.NaN;

    [ObservableProperty] private double _dialogWidth = double.NaN;

    [ObservableProperty] private string _downloadDestinationPath = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DownloadSize))]
    private long _downloadFileSizeBytes = 0;

    [ObservableProperty] private string _iconText = "\xe4e0";

    [ObservableProperty] private bool _isDownloadReady = false;
    [ObservableProperty] private string _message = "Are you sure?";

    [ObservableProperty] private ObservableCollection<NewDownloadItemViewModel> _newDownloadList = new();

    private string _newDownloadUrl = string.Empty;
    [ObservableProperty] private string _progressText = "";
    [ObservableProperty] private string _statusText = "";

    [ObservableProperty] private string _title = "Confirm";

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
        IDialogService dialogService,
        IDownloadService downloadService)
    {
        _dialogService = dialogService;
        _downloadService = downloadService;


        LoadData();
    }

    public string NewDownloadUrl
    {
        get => _newDownloadUrl;
        set
        {
            GetMetadataURLsAsync(value);

            SetProperty(ref _newDownloadUrl, value);
        }
    }

    public double DownloadSize
    {
        get
        {
            double downloadSize = ConversionHelper.ConvertBytesToUnit(DownloadFileSizeBytes, Unit);
            return downloadSize;
        }
    }

    public bool NotBusy() => !Busy;

    private async Task GetMetadataURLsAsync(string urls)
    {
        string[] parts = urls.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            Regex regex =
                new Regex(
                    "(https:\\/\\/www\\.|http:\\/\\/www\\.|https:\\/\\/|http:\\/\\/)?[a-zA-Z0-9]{2,}(\\.[a-zA-Z0-9]{2,})(\\.[a-zA-Z0-9]{2,})?");
            NewDownloadList.Clear();
            foreach (string url in parts)
            {
                if (!regex.IsMatch(url))
                    continue;

                var datamodel = await _downloadService.GetDownloadMetadataAsync(url);
                NewDownloadList.Add(new NewDownloadItemViewModel()
                {
                    FileName = datamodel.FileName,
                    SavePath = datamodel.SavePath,
                    FileSizeInBytes = datamodel.FileSizeInBytes,
                    CreatedAt = DateTime.Now
                });
            }

            DownloadFileSizeBytes = NewDownloadList.Sum(x => x.FileSizeInBytes);
        }
    }

    private void OnDesignTimeConstructor()
    {
        var downloadListMock = new List<NewDownloadItemViewModel>()
        {
            new()
            {
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
        var downloadListMock = new List<NewDownloadItemViewModel>()
        {
            new()
            {
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


    [RelayCommand]
    private async Task SelectDownloadDestinationPathAsync()
    {
        if (string.IsNullOrWhiteSpace(DownloadDestinationPath) ||
            !Path.Exists(Path.GetDirectoryName(DownloadDestinationPath)))
            return;

        string? destinationPath = Path.GetDirectoryName(DownloadDestinationPath);
        FolderPickerOpenOptions options = new()
        {
            Title = "Select download path",
            AllowMultiple = false,
            SuggestedFileName = Path.GetFileName(DownloadDestinationPath)
        };

        destinationPath = await _dialogService.FolderPickerAsync(options);
        if (!string.IsNullOrWhiteSpace(destinationPath))
            DownloadDestinationPath = destinationPath;
    }

    [RelayCommand]
    private async Task StartDownloadLaterAsync()
    {
        var newDownload = await _downloadService.CreateDownloadAsync(NewDownloadUrl, DownloadDestinationPath);
    }

    [RelayCommand]
    private async Task StartNewDownload()
    {
        var newDownload = await _downloadService.CreateDownloadAsync(NewDownloadUrl, DownloadDestinationPath);
    }

    [RelayCommand(CanExecute = nameof(NotBusy))]
    public void Cancel()
    {
        Confirmed = false;
        Close();
    }
}