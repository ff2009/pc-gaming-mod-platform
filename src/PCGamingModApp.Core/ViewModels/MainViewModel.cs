using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Messaging.Messages;

namespace PCGamingModApp.Core.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly IServiceProvider? _serviceProvider = null;
    private readonly PageFactory _pageFactory;

    [ObservableProperty] private ContextViewModel? _currentContext;

    [ObservableProperty] private PageViewModel _currentPage;
    
    private string _filterText;

    public string FilterText
    {
        get { return _filterText; }
        set
        {
            _filterText = value;
            _messenger.Send(new FilterTextMessage(_filterText));
        }
    }

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
        CurrentContext = new MenuViewModel(this, _pageFactory);

        CurrentPage = _pageFactory.GetPageViewModel<BasePageViewModel>();
    }

    [RelayCommand]
    private void Search()
    {
        _messenger.Send(new FilterTextMessage(FilterText));
    }
}