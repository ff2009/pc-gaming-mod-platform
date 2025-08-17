using CommunityToolkit.Mvvm.ComponentModel;
using PCGamingModApp.MainApp;

namespace PCGamingModApp.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ApplicationPageNames _pageName;

    protected PageViewModel(ApplicationPageNames pageName)
    {
        _pageName = pageName;
    }
}
