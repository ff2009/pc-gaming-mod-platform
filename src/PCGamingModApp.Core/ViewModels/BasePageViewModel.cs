using PCGamingModApp.Core.MainApp;

namespace PCGamingModApp.Core.ViewModels;
public partial class BasePageViewModel() : PageViewModel(ApplicationPageNames.Base)
{
    public override string PageTitle => "Base";
}
