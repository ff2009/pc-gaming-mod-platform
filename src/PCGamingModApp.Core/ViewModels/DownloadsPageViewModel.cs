using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class DownloadsPageViewModel : PageViewModel
{
    public override string PageTitle => "Downloads";

    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _downloadsList = [];
    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _filteredDownloadList = [];

    // UI state
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private bool _sortAscending = true;

    public DownloadsPageViewModel() : base(ApplicationPageNames.Downloads)
    {
        // Detect design time
        if (Design.IsDesignMode)
        {
            OnDesignTimeConstructor();
        }
        else
        {
            OnDesignTimeConstructor();
        }

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
}