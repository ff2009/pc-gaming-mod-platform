using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PCGamingModApp.Core.Services.Implementations;

public class GameManagerService(IDialogService dialogService, IGameRepository gameRepository, IGameIconService iconService)
{
    /// <summary>
    /// Adds games to the library
    /// </summary>
    /// <param name="gameExecutablePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<GameDataModel?> AddGame(string gameExecutablePath)
    {
        // Check for duplicates
        var game = await gameRepository.GetGameByInstallPath(gameExecutablePath);
        if (game is not null)
            throw new ArgumentException("Game already registered.");

        // Extract metadata
        var (gameTitle, iconPath) = this.ExtractMetadataAsync(gameExecutablePath);
        if (string.IsNullOrWhiteSpace(gameTitle) || string.IsNullOrWhiteSpace(iconPath))
            return null; // User cancelled or invalid

        string? iconFilename = await iconService.SaveIconAsync(gameTitle, iconPath);
        if (string.IsNullOrWhiteSpace(iconFilename))
            return null; // User cancelled or invalid

        // Convert the VM back to a domain object and hand it to the repo.
        GameDataModel newGame = new()
        {
            Name = gameTitle,
            IconKey = iconFilename,
            InstallPath = gameExecutablePath,
            IsInstalled = true
        };

        await gameRepository.AddGame(newGame);
        return newGame;
    }
    
    private (string? gameTitle, string? iconPath) ExtractMetadataAsync(string gameExecutablePath)
    {
        if (OperatingSystem.IsLinux() && Path.GetExtension(gameExecutablePath) == ".desktop")
        {
            return ExtractLinuxGameInfo(gameExecutablePath);
        }

        // Default: Extract from .exe or fallback to filename
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(gameExecutablePath);
        string gameTitle = !string.IsNullOrEmpty(versionInfo.ProductName)
            ? versionInfo.ProductName
            : Path.GetFileNameWithoutExtension(gameExecutablePath);

        return (gameTitle, gameExecutablePath);
    }

    private (string? gameTitle, string? iconPath) ExtractLinuxGameInfo(string desktopFilePath)
    {
        // Parse .desktop file (simplified example)
        var lines = File.ReadAllLines(desktopFilePath);
        string? iconPath = null;
        string? gameTitle = null;

        foreach (string line in lines)
        {
            if (line.StartsWith("Name="))
                gameTitle = line.Split('=')[1];
            else if (line.StartsWith("Icon="))
                iconPath = line.Split('=')[1];
        }

        // Resolve icon path (e.g., /usr/share/icons/ or ~/.local/share/icons/)
        if (!string.IsNullOrEmpty(iconPath) && !Path.IsPathRooted(iconPath))
        {
            // Search common icon directories
            string[] iconDirs =
            [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "icons"),
                "/usr/share/icons"
            ];

            foreach (var dir in iconDirs)
            {
                string fullPath = Path.Combine(dir, iconPath + ".png");
                if (File.Exists(fullPath))
                {
                    iconPath = fullPath;
                    break;
                }
            }
        }

        return (iconPath, gameTitle);
    }

    /// <summary>
    /// Selects game executable
    /// </summary>
    /// <returns></returns>
    public async Task<string?> SelectGameExecutableAsync()
    {
        List<string> validExtensions = ["*.exe"];
        if (OperatingSystem.IsWindows())
            validExtensions.Add("*.ink"); // Shortcuts

        if (OperatingSystem.IsLinux())
            validExtensions.AddRange(["*.bin", "*.app", "*.desktop"]);

        FilePickerOpenOptions options = new()
        {
            Title = "Select Game Executable",
            FileTypeFilter =
            [
                new FilePickerFileType("Executables")
                {
                    Patterns = validExtensions
                }
            ],
            AllowMultiple = false
        };

        string? result = await dialogService.FilePickerAsync(options);
        return result;
    }
}