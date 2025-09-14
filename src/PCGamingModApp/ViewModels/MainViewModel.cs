using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.MainApp;

namespace PCGamingModApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PageFactory _pageFactory;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private ContextViewModel _currentContext;

    [ObservableProperty] private PageViewModel _currentPage;

    /// <summary>
    /// Design-time only constructor
    /// </summary>
    public MainViewModel()
    {
        if (!Design.IsDesignMode)
            CurrentContext = new GameMenuViewModel();

        CurrentPage = new BasePageViewModel();
    }

    public MainViewModel(IServiceProvider serviceProvider, PageFactory pageFactory)
    {
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
}