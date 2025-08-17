using CommunityToolkit.Mvvm.ComponentModel;
using PCGamingModApp.MainApp;
using System.Collections.ObjectModel;

namespace PCGamingModApp.ViewModels;
public partial class GameListMenuViewModel(MainViewModel mainViewModel) : ContextViewModel
{
    [ObservableProperty]
    private ObservableCollection<string> _gameList = [];
}
