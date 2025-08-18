using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.MainApp;
using System;

namespace PCGamingModApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private PageFactory _pageFactory;

    [ObservableProperty]
    private ContextViewModel _currentContext;

    [ObservableProperty]
    private PageViewModel _currentPage;

    /// <summary>
    /// Design-time only constructor
    /// </summary>
    public MainViewModel()
    {
        if (!Avalonia.Controls.Design.IsDesignMode)
            CurrentContext = new GameListMenuViewModel();

        CurrentPage = new BasePageViewModel();
    }

    public MainViewModel(PageFactory pageFactory)
    {
        _pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));

        CurrentContext = new GameListMenuViewModel(this);
    }


    [RelayCommand]
    private void GoToSettings()
    {
        CurrentContext = new MenuViewModel(this, _pageFactory);

        CurrentPage = _pageFactory.GetPageViewModel<BasePageViewModel>();
    }
}
