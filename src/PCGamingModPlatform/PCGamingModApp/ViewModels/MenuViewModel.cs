using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.MainApp;
using System;

namespace PCGamingModApp.ViewModels;
public partial class MenuViewModel : ContextViewModel
{
    private readonly MainViewModel _mainViewModel;
    private readonly PageFactory _pageFactory;

    [ObservableProperty]
    private bool isDeveloperSettingsVisible;

    /// <summary>
    /// Design-time only constructor
    /// </summary>
    public MenuViewModel()
    {

    }

    public MenuViewModel(MainViewModel mainViewModel, PageFactory pageFactory)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
    }

    [RelayCommand]
    private void GoToBase() => _mainViewModel.CurrentPage = _pageFactory.GetPageViewModel<BasePageViewModel>();

    [RelayCommand]
    private void GoToGameSettings() => _mainViewModel.CurrentPage = _pageFactory.GetPageViewModel<GameSettingsPageViewModel>();

    [RelayCommand]
    private void GoToAddOns() => _mainViewModel.CurrentPage = _pageFactory.GetPageViewModel<AddOnsPageViewModel>();

    [RelayCommand]
    private void GoToSystem() => _mainViewModel.CurrentPage = _pageFactory.GetPageViewModel<SystemPageViewModel>();

    [RelayCommand]
    private void GoToAbout() => _mainViewModel.CurrentPage = _pageFactory.GetPageViewModel<AboutPageViewModel>();
    

    [RelayCommand]
    private void GoToDeveloperSettings() => _mainViewModel.CurrentPage = _pageFactory.GetPageViewModel<DeveloperSettingsPageViewModel>();

}
