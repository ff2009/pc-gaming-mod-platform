using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PCGamingModApp.ViewModels;
public partial class GameListMenuViewModel : ContextViewModel
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private ObservableCollection<GameItemViewModel> _gameList = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedGameItem))]
    private string? _selectedGameItemId;

    private GameItemViewModel? SelectedGameItem =>
        GameList.FirstOrDefault(f => f.Id == SelectedGameItemId);

    public GameListMenuViewModel() : this(new MainViewModel())
    {
        // Detect design time
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            OnDesignTimeConstructor();
        }
    }

    public GameListMenuViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

        OnDesignTimeConstructor();
    }

    private void OnDesignTimeConstructor()
    {
        GameList = [
                new GameItemViewModel { Id = "-1", Title = "Back to Dinosaur Island", Cover = "https://images.pcgamingwiki.com/e/e2/Back_to_Dinosaur_Island_cover.jpg"},
                //new GameItemViewModel { Id = "-2", Title = "Back to Dinosaur Island Part 2", Cover = "https://images.pcgamingwiki.com/2/2e/Back_to_Dinosaur_Island_Part_2_cover.jpg"},
                new GameItemViewModel { Id = "-3", Title = "Crysis", Cover = "https://images.pcgamingwiki.com/2/22/Crysis_cover.jpg"},
                new GameItemViewModel { Id = "-4", Title = "Crysis 2", Cover = "https://images.pcgamingwiki.com/7/76/Crysis_2_cover.jpg"},
                new GameItemViewModel { Id = "-5", Title = "Crysis 2 Remastered", Cover = "https://images.pcgamingwiki.com/d/d1/Crysis_2_Remastered_cover.jpg"},
                new GameItemViewModel { Id = "-6", Title = "Crysis 3", Cover = "https://images.pcgamingwiki.com/2/20/Crysis_3_cover.jpg"},
                //new GameItemViewModel { Id = "-5", Title = "Crysis 3 Remastered", Cover = "https://images.pcgamingwiki.com/1/15/Crysis_3_Remastered_cover.jpg"},
                new GameItemViewModel { Id = "-7", Title = "Crysis 3 Remastered", Cover = null },
            ];
    }
}
