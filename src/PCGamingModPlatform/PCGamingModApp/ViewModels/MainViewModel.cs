using CommunityToolkit.Mvvm.ComponentModel;

namespace PCGamingModApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting { get; } = "PC Gaming Mod App";

    [ObservableProperty]
    private ViewModelBase _currentContext;

    [ObservableProperty]
    private PageViewModel _currentPage;
}
