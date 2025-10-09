using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Messaging.Messages;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.ViewModels;

/// <summary>
/// View‑model for a single row in the Game‑Menu list.
/// </summary>
public partial class GameItemViewModel : ViewModelBase
{
    private readonly GameManagerService _gameManagerService;
    private readonly IGameRepository _gameRepository;
    private readonly IImageCache _imageCache;
    private readonly ILauncherService _launcherService;
    private readonly IMessenger _messenger;


    /// <summary>
    /// Image handling – only the key is stored
    /// </summary>
    [ObservableProperty] private string _iconKey;

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string? _installPath; // null when not installed
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private bool _isInstalled;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private StoreType _store = StoreType.Unknown;

    public GameItemViewModel(IImageCache imageCache, GameDataModel domain)
    {
        // services
        _imageCache = imageCache;

        // immutable fields
        Id = domain.Id;
        IconKey = domain.IconKey;

        // mutable fields – go through the generated setters so change‑notification works
        Name = domain.Name;
        Store = domain.Store;
        IsInstalled = domain.IsInstalled;
        IsFavorite = domain.IsFavorite;
        InstallPath = domain.InstallPath;
    }

    public GameItemViewModel(
        GameManagerService gameManagerService,
        IGameRepository gameRepository,
        IImageCache imageCache,
        ILauncherService launcherService,
        IMessenger messenger,
        GameDataModel domain
    ) : this(imageCache, domain)
    {
        // services
        _gameManagerService =
            gameManagerService ?? throw new ArgumentNullException(nameof(gameManagerService));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
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
            await _launcherService.LaunchAsync(InstallPath);
        }
    }

    private bool CanPlay() => IsInstalled && !string.IsNullOrWhiteSpace(InstallPath);

    /// <summary>
    /// Add – used for catalog‑only entries (or manual add)
    /// </summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var gameExecutablePath = await _gameManagerService.SelectGameExecutableAsync();

        if (string.IsNullOrWhiteSpace(gameExecutablePath))
        {
            // user cancelled or no valid selection
            return;
        }
        
        _gameManagerService.SaveGameIcon(gameExecutablePath);
        
        InstallPath = gameExecutablePath;
        IsInstalled = true;
        
        // Convert the VM back to a domain object and hand it to the repo.
        var newItem = ToDomain();
        await  _gameRepository.AddGame(newItem);
        _messenger.Send(new GameAddedMessage(newItem));
    }

    /// <summary>
    /// Edit – opens a dialog elsewhere; we just forward the request
    /// </summary>
    [RelayCommand]
    private void Edit()
    {
        var updatedItem = ToDomain();
        _gameRepository.UpdateGame(updatedItem);
        _messenger.Send(new GameAddedMessage(updatedItem));
    }

    /// <summary>
    /// Delete – removes the entry from the repository
    /// </summary>
    [RelayCommand]
    private void Delete()
    {
        _gameRepository.DeleteGame(Id);
        _messenger.Send(new GameDeletedMessage(Id));
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
    private GameDataModel ToDomain() => new()
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