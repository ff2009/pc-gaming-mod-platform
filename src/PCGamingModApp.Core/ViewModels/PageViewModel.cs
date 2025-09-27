using CommunityToolkit.Mvvm.ComponentModel;
using PCGamingModApp.Core.MainApp;

namespace PCGamingModApp.Core.ViewModels;

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
