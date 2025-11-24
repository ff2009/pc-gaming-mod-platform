using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadsPageViewModel : PageViewModel
{
    public override string PageTitle => "Downloads";

    private readonly IDownloadService _downloadService;
    private readonly IServiceProvider? _serviceProvider;

    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _downloadsList = [];
    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _filteredDownloadList = [];

    // UI state
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private bool _sortAscending = true;

    [ObservableProperty] private bool _isShowingAllDownloads = true;
    [ObservableProperty] private bool _isShowingActiveDownloads = false;
    [ObservableProperty] private bool _isShowingCompletedDownloads = false;
    [ObservableProperty] private bool _isShowingNewDownload = false;

    [ObservableProperty] private string _newDownloadUrl = string.Empty;

    [ObservableProperty] private string _newDownloadDestinationPath = string.Empty;
    [ObservableProperty] private double _newDownloadFileSizeBytes = 0;
    [ObservableProperty] private bool _isDownloadReady = false;

    // Design-time constructor
    public DownloadsPageViewModel() : base(ApplicationPageNames.Downloads)
    {
        if (Design.IsDesignMode)
            OnDesignTimeConstructor();

        ApplyFiltersAndSorting();
    }

    public DownloadsPageViewModel(IDownloadService downloadService, IServiceProvider serviceProvider) : base(
        ApplicationPageNames.Downloads)
    {
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        OnDesignTimeConstructor();

        ApplyFiltersAndSorting();
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
            .Select(dm => new DownloadItemViewModel(new DownloadService(null, null), dm))
            .ToList();
        DownloadsList = new ObservableCollection<DownloadItemViewModel>(vmList);
    }

    public async Task GetMetadata()
    {
        // Simulate fetching metadata for the new download URL
        if (Uri.IsWellFormedUriString(NewDownloadUrl, UriKind.Absolute))
        {
            var datamodel = await _downloadService.GetDownloadMetadataAsync(NewDownloadUrl);
            // For design time, we just set a mock file size
            NewDownloadFileSizeBytes = datamodel.FileSizeInBytes; // 150 MB
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
    private void SelectDownloadDestinationPath()
    {
    }

    [RelayCommand]
    private void StartDownloadLater()
    {
        IsShowingNewDownload = false;
    }

    [RelayCommand]
    private void StartNewDownload()
    {
        IsShowingNewDownload = false;
    }

    [RelayCommand]
    private void CancelNewDownload()
    {
        IsShowingNewDownload = false;
    }
}