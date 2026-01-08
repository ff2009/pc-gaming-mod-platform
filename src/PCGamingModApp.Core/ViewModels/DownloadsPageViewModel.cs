using System.Collections.ObjectModel;
using Avalonia.Controls;
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
    private readonly IDownloadOrchestrator _downloadOrchestrator;
    private readonly IMessenger _messenger;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private double _currentDownloadSpeed = 0;

    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _downloadsList = [];
    [ObservableProperty] private ObservableCollection<DownloadItemViewModel> _filteredDownloadList = [];

    // UI state
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private bool _isShowingActiveDownloads = false;

    [ObservableProperty] private bool _isShowingAllDownloads = true;
    [ObservableProperty] private bool _isShowingCompletedDownloads = false;
    [ObservableProperty] private double _peakDownloadSpeed = 25;

    [ObservableProperty] private double _sessionTraffic = 0;
    [ObservableProperty] private bool _sortAscending = true;
    [ObservableProperty] private double _totalTraffic = 0;
    [ObservableProperty] private SizeUnit _trafficUnit = SizeUnit.GB; // in bytes per second

    [ObservableProperty] private SizeUnit _unit = SizeUnit.MB; // in bytes per second

    // Design-time constructor
    public DownloadsPageViewModel() : base(ApplicationPageNames.Downloads)
    {
        if (Design.IsDesignMode)
            OnDesignTimeConstructor();

        ApplyFiltersAndSorting();
    }

    public DownloadsPageViewModel(
        IDialogService dialogService,
        IDownloadOrchestrator downloadOrchestrator,
        IMessenger messenger,
        IServiceProvider serviceProvider) : base(ApplicationPageNames.Downloads)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _downloadOrchestrator = downloadOrchestrator ?? throw new ArgumentNullException(nameof(downloadOrchestrator));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _messenger.RegisterAll(this);
        LoadData();

        ApplyFiltersAndSorting();
    }

    public override string PageTitle => "Downloads";

    public void Receive(DownloadAddedMessage message)
    {
        var downloadItem = ActivatorUtilities.CreateInstance<DownloadItemViewModel>(
            _serviceProvider,
            message.DownloadService,  // Pass the service
            _messenger);               // Pass the messenger

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
            .Select(dm => new DownloadItemViewModel(new SingleDownloadService(dm, null,null,null,null,null), null))
            .ToList();
        DownloadsList = new ObservableCollection<DownloadItemViewModel>(vmList);
    }

    private void LoadData()
    {
        var downloads = _downloadOrchestrator.GetDownloadServices();
        if (!downloads.Any())
            return;

        List<DownloadItemViewModel> vmList = downloads
            .Select(g => ActivatorUtilities.CreateInstance<DownloadItemViewModel>(_serviceProvider, g)).ToList();
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
    private async Task NewDownloadAsync()
    {
        var confirmViewModel = _serviceProvider.GetRequiredService<NewDownloadDialogViewModel>();
        confirmViewModel.Title = "New Download";
        {
            //OnConfirm = async (vm) => {
            //    await Task.Delay(2000);

            //    vm.ProgressText = "This is taking a while...";

            //    await Task.Delay(2000);

            //    vm.StatusText = "Oh no, something went wrong...";

            //    return true;
            //}
        };

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        await _dialogService.ShowDialog(mainViewModel, confirmViewModel);

        // Ignore if we clicked cancel
        if (!confirmViewModel.Confirmed)
            return;
    }


    [RelayCommand]
    private void StartNewDownload()
    {
        /*var newDownload = await _downloadService.CreateDownloadAsync(NewDownloadUrl, NewDownloadDestinationPath);
        var download = ActivatorUtilities.CreateInstance<DownloadItemViewModel>(_serviceProvider, newDownload);
        DownloadsList.Add(download);*/

        ApplyFiltersAndSorting();
    }

    [RelayCommand]
    private void GoToDownloadSettings()
    {
        // TODO
    }
}