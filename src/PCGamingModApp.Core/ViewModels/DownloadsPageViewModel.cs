using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadsPageViewModel : PageViewModel, IRecipient<DownloadAddedMessage>,
    IRecipient<DownloadDeletedMessage>
{
    private readonly IDialogService _dialogService;
    private readonly IDownloadService _downloadService;
    private readonly IMessenger _messenger;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _downloadsList = [];
    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _filteredDownloadList = [];

    // UI state
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private bool _isDownloadReady = false;
    [ObservableProperty] private bool _isShowingActiveDownloads = false;

    [ObservableProperty] private bool _isShowingAllDownloads = true;
    [ObservableProperty] private bool _isShowingCompletedDownloads = false;
    [ObservableProperty] private bool _isShowingNewDownload = false;
    [ObservableProperty] private string _newDownloadDestinationPath = string.Empty;

    [ObservableProperty] private string _newDownloadFilename = string.Empty;
    [ObservableProperty] private double _newDownloadFileSizeBytes = 0;

    [ObservableProperty] private string _newDownloadUrl = string.Empty;
    [ObservableProperty] private bool _sortAscending = true;

    // Design-time constructor
    public DownloadsPageViewModel() : base(ApplicationPageNames.Downloads)
    {
        if (Design.IsDesignMode)
            OnDesignTimeConstructor();

        ApplyFiltersAndSorting();
    }

    public DownloadsPageViewModel(
        IDialogService dialogService,
        IDownloadService downloadService,
        IMessenger messenger,
        IServiceProvider serviceProvider) : base(ApplicationPageNames.Downloads)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _messenger.RegisterAll(this);
        _ = LoadData();

        ApplyFiltersAndSorting();
    }

    public override string PageTitle => "Downloads";

    public void Receive(DownloadAddedMessage message)
    {
        var download = message.Download;
        var downloadItem = ActivatorUtilities.CreateInstance<DownloadItemViewModel>(_serviceProvider, download);
        DownloadsList.Add(downloadItem);
        ApplyFiltersAndSorting();
    }

    public void Receive(DownloadDeletedMessage message)
    {
        var downloadToRemove = DownloadsList.FirstOrDefault(g => g.Id == message.DownloadId);
        if (downloadToRemove != null)
        {
            DownloadsList.Remove(downloadToRemove);
            ApplyFiltersAndSorting();
        }
    }

    private void OnDesignTimeConstructor()
    {
        var downloadListMock = new List<DownloadDataModel>()
        {
            new()
            {
                FileName = "OptiScaler_0.7.9.7z",
                FileSizeInBytes = 123456789,
                DownloadedBytes = 0,
                Status = DownloadStatus.Pending,
                CreatedAt = DateTime.Now
            },
            new()
            {
                FileName = "Optiscaler_0.9.0-pre5 (20251031).7z",
                FileSizeInBytes = 324535560,
                DownloadedBytes = 250525435,
                Status = DownloadStatus.InProgress,
                CreatedAt = DateTime.Now.AddDays(-5)
            },
            new()
            {
                FileName = "dlssg-to-fsr3-0.130-738-0-130-1742150748.zip",
                FileSizeInBytes = 23452352,
                DownloadedBytes = 5205677,
                Status = DownloadStatus.InProgress,
                CreatedAt = DateTime.Now.AddDays(-7)
            },
            new()
            {
                FileName = "FidelityFX-SDK-v1.1.4.zip",
                FileSizeInBytes = 223413453,
                DownloadedBytes = 223413453,
                Status = DownloadStatus.Completed,
                CreatedAt = DateTime.Now.AddDays(-1)
            },
        };

        List<DownloadItemViewModel> vmList = downloadListMock
            .Select(dm => new DownloadItemViewModel(new DownloadService(null, null, null, null, null), null, dm))
            .ToList();
        DownloadsList = new ObservableCollection<DownloadItemViewModel>(vmList);
    }

    private async Task LoadData()
    {
        var downloads = await _downloadService.GetDownloadsAsync();
        if (!downloads.Any())
            return;

        List<DownloadItemViewModel> vmList = downloads
            .Select(g => ActivatorUtilities.CreateInstance<DownloadItemViewModel>(_serviceProvider, g)).ToList();
        DownloadsList = new ObservableCollection<DownloadItemViewModel>(vmList);
    }

    public async Task GetMetadata()
    {
        // Simulate fetching metadata for the new download URL
        if (Uri.IsWellFormedUriString(NewDownloadUrl, UriKind.Absolute))
        {
            var datamodel = await _downloadService.GetDownloadMetadataAsync(NewDownloadUrl);
            NewDownloadFileSizeBytes = datamodel.FileSizeInBytes;
            NewDownloadDestinationPath = datamodel.SavePath;
            NewDownloadFilename = datamodel.FileName;
            IsDownloadReady = true;
            return;
        }

        NewDownloadFileSizeBytes = 0;
        IsDownloadReady = false;
    }

    /// <summary>
    /// Filtering / Sorting logic (called whenever a related property changes)
    /// </summary>
    [RelayCommand]
    private void ApplyFiltersAndSorting()
    {
        var query = DownloadsList.AsEnumerable();

        if (!string.IsNullOrEmpty(FilterText))
            query = query.Where(game => game.FileName.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        FilteredDownloadList = new ObservableCollection<DownloadItemViewModel>(SortAscending
            ? query.OrderBy(game => game.FileName)
            : query.OrderByDescending(game => game.FileName));
    }

    [RelayCommand]
    private void ShowAllDownloads()
    {
        IsShowingAllDownloads = true;
    }

    [RelayCommand]
    private void ShowActiveDownloads()
    {
    }

    [RelayCommand]
    private void ShowCompletedDownloads()
    {
    }

    [RelayCommand]
    private void NewDownload()
    {
        IsShowingNewDownload = true;
    }

    [RelayCommand]
    private async Task SelectDownloadDestinationPathAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDownloadDestinationPath) ||
            !Path.Exists(Path.GetDirectoryName(NewDownloadDestinationPath)))
            return;

        string? destinationPath = Path.GetDirectoryName(NewDownloadDestinationPath);
        FolderPickerOpenOptions options = new()
        {
            Title = "Select download path",
            AllowMultiple = false,
            SuggestedFileName = Path.GetFileName(NewDownloadDestinationPath)
        };

        destinationPath = await _dialogService.FolderPickerAsync(options);
        if (!string.IsNullOrWhiteSpace(destinationPath))
            NewDownloadDestinationPath = Path.Combine(destinationPath, NewDownloadFilename);
    }

    [RelayCommand]
    private async Task StartDownloadLaterAsync()
    {
        var newDownload = await _downloadService.CreateDownloadAsync(NewDownloadUrl, NewDownloadDestinationPath);
        var download = ActivatorUtilities.CreateInstance<DownloadItemViewModel>(_serviceProvider, newDownload);
        DownloadsList.Add(download);
        IsShowingNewDownload = false;
        ApplyFiltersAndSorting();
    }

    [RelayCommand]
    private async Task StartNewDownload()
    {
        var newDownload = await _downloadService.CreateDownloadAsync(NewDownloadUrl, NewDownloadDestinationPath);
        var download = ActivatorUtilities.CreateInstance<DownloadItemViewModel>(_serviceProvider, newDownload);
        DownloadsList.Add(download);
        IsShowingNewDownload = false;
        ApplyFiltersAndSorting();
    }

    [RelayCommand]
    private void CancelNewDownload()
    {
        IsShowingNewDownload = false;
    }
}