using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Core.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    private readonly IMessenger _messenger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PageFactory _pageFactory;

    [ObservableProperty] private ContextViewModel? _currentContext;

    [ObservableProperty] private PageViewModel _currentPage;
    
    [ObservableProperty]
    private DialogViewModel _dialog;
    
    private string _filterText;

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText == value) return;
            _filterText = value;
            _messenger.Send(new FilterTextMessage(_filterText));
        }
    }
    
    [ObservableProperty]
    private bool _sideMenuExpanded = true;
    
    [ObservableProperty]
    private bool _sideMenuGameMenuVisible = true;

    /// <summary>
    /// Design-time only constructor
    /// </summary>
    public MainViewModel()
    {
        CurrentContext = new GameMenuViewModel();

        CurrentPage = new BasePageViewModel();
    }

    public MainViewModel(IMessenger messenger, IServiceProvider serviceProvider, PageFactory pageFactory)
    {
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));

        CurrentContext = _serviceProvider.GetRequiredService<GameMenuViewModel>();
    }


    [RelayCommand]
    private void GoToSettings()
    {
        CurrentContext = _serviceProvider.GetRequiredService<MenuViewModel>();

        CurrentPage = _pageFactory.GetPageViewModel<BasePageViewModel>();
        
        SideMenuGameMenuVisible = false;
    }

    [RelayCommand]
    private void GoBack()
    {
        CurrentContext = _serviceProvider.GetRequiredService<GameMenuViewModel>();

        CurrentPage = _pageFactory.GetPageViewModel<BasePageViewModel>();
        
        SideMenuGameMenuVisible = true;
    }
    
    [RelayCommand]
    private void Search()
    {
        _messenger.Send(new FilterTextMessage(FilterText));
    }
}