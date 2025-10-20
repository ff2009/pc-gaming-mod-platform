using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;
using System.Collections.ObjectModel;

namespace PCGamingModApp.Core.ViewModels;

public partial class GameMenuViewModel : ContextViewModel, IRecipient<GameAddedMessage>, IRecipient<GameUpdatedMessage>,
    IRecipient<GameDeletedMessage>, IDisposable
{
    private readonly IGameRepository _gameRepository;
    private readonly IMessenger _messenger;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private ObservableCollection<GameItemViewModel> _gamesList = [];
    [ObservableProperty] private ObservableCollection<GameItemViewModel> _filteredGameList;


    // UI state
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private bool _showFavoritesOnly;
    [ObservableProperty] private bool _showInstalledOnly;
    [ObservableProperty] private bool _sortAscending = true;

    public GameMenuViewModel()
    {
        // Detect design time
        if (Design.IsDesignMode)
        {
            OnDesignTimeConstructor();
        }

        ApplyFiltersAndSorting();
    }

    public GameMenuViewModel(
        IGameRepository gameRepository,
        IMessenger messenger,
        IServiceProvider serviceProvider)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _messenger.RegisterAll(this);
        LoadData();
        ApplyFiltersAndSorting();
    }

    private void OnDesignTimeConstructor()
    {
        var gameListMock = new List<GameDataModel>()
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Back to Dinosaur Island", IconKey = "game-controller.png",
                Store = StoreType.Epic,
                InstallPath = @"C:\Users\ff2009\Downloads\GPU-Z.2.66.0.exe", IsInstalled = true, IsFavorite = true
            },
            new() { Id = Guid.NewGuid(), Name = "Crysis", IconKey = "game.png", Store = StoreType.EaApp },
            new() { Id = Guid.NewGuid(), Name = "Crysis 2", IconKey = "game-controller.png", Store = StoreType.Other },
            new() { Id = Guid.NewGuid(), Name = "Crysis 2 Remastered", IconKey = "game.png", Store = StoreType.Steam },
            new() { Id = Guid.NewGuid(), Name = "Crysis 3", IconKey = "game-control.png", Store = StoreType.Ubisoft },
            new()
            {
                Id = Guid.NewGuid(), Name = "Spacewars", IconKey = "game-controller.png", Store = StoreType.Steam,
                IsFavorite = true
            },
            new() { Id = Guid.NewGuid(), Name = "The Witcher 3", IconKey = "game-control.png", Store = StoreType.GoG },
        };

        DesignTimeImageCache imageCache = new("Assets/Images/");
        List<GameItemViewModel> vmList = gameListMock.Select(g => new GameItemViewModel(imageCache, g)).ToList();
        GamesList = new ObservableCollection<GameItemViewModel>();
    }

    private async Task LoadData()
    {
        var games = await _gameRepository.GetAllGames();
        if (games == null || !games.Any())
        {
            var gameListMock = new List<GameDataModel>()
            {
                new()
                {
                    Id = Guid.NewGuid(), Name = "Back to Dinosaur Island", IconKey = "game-controller.png",
                    Store = StoreType.Epic,
                    InstallPath = @"C:\Users\ff2009\Downloads\GPU-Z.2.66.0.exe", IsInstalled = true, IsFavorite = true
                },
                new() { Id = Guid.NewGuid(), Name = "Crysis", IconKey = "game.png", Store = StoreType.EaApp },
                new()
                {
                    Id = Guid.NewGuid(), Name = "Crysis 2", IconKey = "game-controller.png", Store = StoreType.Other
                },
                new()
                {
                    Id = Guid.NewGuid(), Name = "Crysis 2 Remastered", IconKey = "game.png", Store = StoreType.Steam
                },
                new()
                {
                    Id = Guid.NewGuid(), Name = "Crysis 3", IconKey = "game-control.png", Store = StoreType.Ubisoft
                },
                new()
                {
                    Id = Guid.NewGuid(), Name = "Spacewars", IconKey = "game-controller.png", Store = StoreType.Steam,
                    IsFavorite = true
                },
                new()
                {
                    Id = Guid.NewGuid(), Name = "The Witcher 3", IconKey = "game-control.png", Store = StoreType.GoG
                },
            };
            games.AddRange(gameListMock);
        }


        List<GameItemViewModel> vmList = games
            .Select(g => ActivatorUtilities.CreateInstance<GameItemViewModel>(_serviceProvider, g)).ToList();
        GamesList = new ObservableCollection<GameItemViewModel>(vmList);
    }

    /// <summary>
    /// Filtering / Sorting logic (called whenever a related property changes)
    /// </summary>
    [RelayCommand]
    private void ApplyFiltersAndSorting()
    {
        var query = GamesList.AsEnumerable();

        if (!string.IsNullOrEmpty(FilterText))
            query = query.Where(game => game.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        if (ShowFavoritesOnly)
            query = query.Where(game => game.IsFavorite);

        FilteredGameList = new ObservableCollection<GameItemViewModel>(SortAscending? query.OrderBy(game => game.Name) : query.OrderByDescending(game => game.Name));
    }

    // -----------------------------------------------------------------
    // Top‑bar commands
    // -----------------------------------------------------------------
    [RelayCommand]
    private void AddNewGame()
    {
        // Open a modal dialog (implementation left to UI layer)
        // After the dialog returns a GameItem, call _repo.AddAsync and refresh.
    }

    [RelayCommand]
    private void ToggleFavorites()
    {
        ShowFavoritesOnly = !ShowFavoritesOnly;
        ApplyFiltersAndSorting();
    }
    
    [RelayCommand]
    private void ToggleSortOrder()
    {
        SortAscending = !SortAscending;
        ApplyFiltersAndSorting();
    }

    public void Receive(GameAddedMessage message)
    {
        var gameToUpdate = GamesList.FirstOrDefault(g => g.Id == message.Game.Id);
        if (gameToUpdate != null)
        {
            // Update other properties as needed
            gameToUpdate.IsInstalled = true;
        }
    }

    public void Receive(GameUpdatedMessage message)
    {
        var gameToUpdate = GamesList.FirstOrDefault(g => g.Id == message.Game.Id);
        if (gameToUpdate != null)
        {
            // Update other properties as needed
            gameToUpdate.IsInstalled = !gameToUpdate.IsInstalled;
        }
    }

    public void Receive(GameDeletedMessage message)
    {
        var gameToRemove = GamesList.FirstOrDefault(g => g.Id == message.GameId);
        if (gameToRemove != null)
        {
            // Update other properties as needed
            gameToRemove.IsInstalled = false;
            GamesList.Remove(gameToRemove);
        }
    }

    public void Dispose()
    {
        _messenger.UnregisterAll(this);
    }
}