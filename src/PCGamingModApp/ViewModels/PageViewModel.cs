using CommunityToolkit.Mvvm.ComponentModel;
using PCGamingModApp.MainApp;

namespace PCGamingModApp.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ApplicationPageNames _pageName;

    public virtual string PageTitle { get; } = "Page Title";

    protected PageViewModel(ApplicationPageNames pageName)
    {
        _pageName = pageName;
    }
}
