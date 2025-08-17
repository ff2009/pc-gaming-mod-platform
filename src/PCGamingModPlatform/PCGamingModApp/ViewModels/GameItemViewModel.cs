using CommunityToolkit.Mvvm.ComponentModel;

namespace PCGamingModApp.ViewModels;
public partial class GameItemViewModel : ViewModelBase
{
    [ObservableProperty] 
    private string _id = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _cover = string.Empty;

}
