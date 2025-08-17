using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.MainApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGamingModApp.ViewModels;
public partial class MenuViewModel : ContextViewModel
{
    private readonly MainViewModel _mainViewModel;
    private readonly PageFactory _pageFactory;

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

}
