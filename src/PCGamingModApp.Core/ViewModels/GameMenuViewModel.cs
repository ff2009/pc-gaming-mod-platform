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

    [ObservableProperty] private ObservableCollection<GameItemViewModel> _games = [];

    // UI state
    [ObservableProperty] private string _searchText = string.Empty;
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
        Games = new ObservableCollection<GameItemViewModel>(vmList);
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
        Games = new ObservableCollection<GameItemViewModel>(vmList);
    }

    /// <summary>
    /// Filtering / Sorting logic (called whenever a related property changes)
    /// </summary>
    [RelayCommand]
    private void ApplyFiltersAndSorting()
    {
        // var view = CollectionViewSource.GetDefaultView(Games);
        // view.Filter = o =>
        // {
        //     if (o is not GameItemViewModel g) return false;
        //     if (ShowFavoritesOnly && !g.IsFavorite) return false;
        //     if (ShowInstalledOnly && !g.IsInstalled) return false;
        //     if (!string.IsNullOrWhiteSpace(SearchText) &&
        //         !g.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        //         return false;
        //     return true;
        // };
        //
        // view.SortDescriptions.Clear();
        // view.SortDescriptions.Add(new SortDescription(nameof(GameItemViewModel.Name),
        //     SortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
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
    private void ToggleSortOrder()
    {
        SortAscending = !SortAscending;
        ApplyFiltersAndSorting();
    }

    public void Receive(GameAddedMessage message)
    {
        var gameToUpdate = Games.FirstOrDefault(g => g.Id == message.Game.Id);
        if (gameToUpdate != null)
        {
            // Update other properties as needed
            gameToUpdate.IsInstalled = true;
        }
    }

    public void Receive(GameUpdatedMessage message)
    {
        var gameToUpdate = Games.FirstOrDefault(g => g.Id == message.Game.Id);
        if (gameToUpdate != null)
        {
            // Update other properties as needed
            gameToUpdate.IsInstalled = !gameToUpdate.IsInstalled;
        }
    }

    public void Receive(GameDeletedMessage message)
    {
        var gameToRemove = Games.FirstOrDefault(g => g.Id == message.GameId);
        if (gameToRemove != null)
        {
            // Update other properties as needed
            gameToRemove.IsInstalled = false;
            Games.Remove(gameToRemove);
        }
    }

    public void Dispose()
    {
        _messenger.UnregisterAll(this);
    }
}