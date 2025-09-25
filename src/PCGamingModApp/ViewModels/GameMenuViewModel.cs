using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Models;
using PCGamingModApp.Models.Enums;
using PCGamingModApp.Services.Implementations;
using PCGamingModApp.Services.Interfaces;

namespace PCGamingModApp.ViewModels;

public partial class GameMenuViewModel : ContextViewModel
{
    //private readonly IGameRepository _repo;
    private readonly IImageCache _imageCache;
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
        //IGameRepository repo,
        IImageCache imageCache,
        IServiceProvider serviceProvider)
    {
        //_repo        = repo ?? throw new ArgumentNullException(nameof(repo));
        _imageCache = imageCache ?? throw new ArgumentNullException(nameof(imageCache));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        OnDesignTimeConstructor();
    }

    private void OnDesignTimeConstructor()
    {
        var gameListMock = new List<GameItem>()
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Back to Dinosaur Island", IconKey = "game-controller.png", Store = eStoreType.Epic,
                InstallPath = @"C:\Users\ff2009\Downloads\GPU-Z.2.66.0.exe", IsInstalled = true, IsFavorite = true
            },
            new() { Id = Guid.NewGuid(), Name = "Crysis", IconKey = "game.png", Store = eStoreType.EaApp },
            new() { Id = Guid.NewGuid(), Name = "Crysis 2", IconKey = "game-controller.png", Store = eStoreType.Other },
            new() { Id = Guid.NewGuid(), Name = "Crysis 2 Remastered", IconKey = "game.png", Store = eStoreType.Steam },
            new() { Id = Guid.NewGuid(), Name = "Crysis 3", IconKey = "game-control.png", Store = eStoreType.Ubisoft },
            new() { Id = Guid.NewGuid(), Name = "Spacewars", IconKey = "game-controller.png", Store = eStoreType.Steam, IsFavorite = true },
            new() { Id = Guid.NewGuid(), Name = "The Witcher 3", IconKey = "game-control.png", Store = eStoreType.GoG },
        };

        var imageCache = new DesignTimeImageCache("Assets/Images/");

        List<GameItemViewModel> vmList = gameListMock.Select(g => new GameItemViewModel(g, imageCache)).ToList();
        //vmList = gameListMock.Select(g => ActivatorUtilities.CreateInstance<GameItemViewModel>(_serviceProvider, g)).ToList();

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
}