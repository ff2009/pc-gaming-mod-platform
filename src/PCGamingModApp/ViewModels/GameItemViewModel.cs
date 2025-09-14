using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCGamingModApp.Models;
using PCGamingModApp.Models.Enums;
using PCGamingModApp.Services.Interfaces;

namespace PCGamingModApp.ViewModels;

/// <summary>
/// View‑model for a single row in the Game‑Menu list.
/// </summary>
public partial class GameItemViewModel : ViewModelBase
{
    private readonly IImageCache _imageCache;
    private readonly ILauncherService _launcher;

    /// <summary>
    /// Image handling – only the key is stored
    /// </summary>
    [ObservableProperty] private string _iconKey;

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string? _installPath; // null when not installed
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private bool _isInstalled;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private eStoreType _store = eStoreType.Unknown;

    public GameItemViewModel(
        GameItem domain,
        ILauncherService launcher,
        //IGameRepository repository,
        IImageCache imageCache)
    {
        // immutable fields
        Id = domain.Id;
        IconKey = domain.IconKey;

        // mutable fields – go through the generated setters so change‑notification works
        Name = domain.Name;
        Store = domain.Store;
        IsInstalled = domain.IsInstalled;
        IsFavorite = domain.IsFavorite;
        InstallPath = domain.InstallPath;

        // services
        _launcher = launcher;
        //_repository = repository;
        _imageCache = imageCache;
    }

    /// <summary>
    /// Computed read‑only property – the UI binds to this
    /// </summary>
    public Bitmap? Icon => _imageCache.GetBitmap(IconKey);

    /// <summary>
    /// Play – only enabled when the game is installed and we have a path
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPlay))]
    private async Task PlayAsync()
    {
        if (!string.IsNullOrWhiteSpace(InstallPath))
        {
            await _launcher.LaunchAsync(InstallPath);
        }
    }

    private bool CanPlay() => IsInstalled && !string.IsNullOrWhiteSpace(InstallPath);

    /// <summary>
    /// Add – used for catalog‑only entries (or manual add)
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        // Convert the VM back to a domain object and hand it to the repo.
        var newItem = ToDomain();
        //_repository.Add(newItem);
    }

    /// <summary>
    /// Edit – opens a dialog elsewhere; we just forward the request
    /// </summary>
    [RelayCommand]
    private void Edit()
    {
        //_repository.Edit(ToDomain());
    }

    /// <summary>
    /// Delete – removes the entry from the repository
    /// </summary>
    [RelayCommand]
    private void Delete()
    {
        //_repository.Remove(Id);
    }

    /// <summary>
    /// Toggle favorite flag
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }

    /// <summary>
    /// Helper – map back to the domain model (useful for persistence)
    /// </summary>
    /// <returns></returns>
    public GameItem ToDomain() => new GameItem
    {
        Id = Id,
        Name = Name,
        Store = Store,
        IsInstalled = IsInstalled,
        IsFavorite = IsFavorite,
        InstallPath = InstallPath,
        IconKey = IconKey
    };
}