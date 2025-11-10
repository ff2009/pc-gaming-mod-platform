using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;
using SkiaSharp;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace PCGamingModApp.Core.Services.Implementations;

public class GameManagerService(IAppPaths appPaths, IDialogService dialogService, IGameRepository gameRepository)
{
    /// <summary>
    /// Adds games to the library
    /// </summary>
    /// <param name="gameExecutablePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<GameDataModel?> AddGame(string gameExecutablePath)
    {
        var game = await gameRepository.GetGameByInstallPath(gameExecutablePath);
        if (game is not null)
        {
            throw new ArgumentException("Game already registered in the library.");
        }

        // Extract game title (ProductName from version info)
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(gameExecutablePath);
        string gameTitle = !string.IsNullOrEmpty(versionInfo.ProductName)
            ? versionInfo.ProductName
            : Path.GetFileNameWithoutExtension(gameExecutablePath);

        if (OperatingSystem.IsLinux())
        {
            var (iconPathTemp, gameTitleTemp) = ExtractLinuxGameInfo(gameExecutablePath);
            if (!string.IsNullOrWhiteSpace(iconPathTemp))
                gameExecutablePath = iconPathTemp;

            if (!string.IsNullOrWhiteSpace(gameTitleTemp))
                gameTitle = gameTitleTemp;
        }

        string iconFilename = $"{gameTitle.Trim().ToLowerInvariant().Replace(' ', '-')}-{Guid.NewGuid():N}.png";
        string outputPath = Path.Combine(appPaths.GameIcons, iconFilename);

        await SaveGameIconAsync(gameExecutablePath, outputPath);

        if (string.IsNullOrWhiteSpace(gameTitle))
        {
            // user cancelled or no valid selection
            return null;
        }

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
    
    private (string? iconPath, string? gameTitle) ExtractLinuxGameInfo(string desktopFilePath)
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
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share",
                    "icons"),
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
    /// Extracts the icon from the <see cref="sourcePath"/> file and stores it to the <see cref="outputPath"/> file
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="outputPath"></param>
    private async Task SaveGameIconAsync(string sourcePath, string outputPath)
    {
        await using MemoryStream memoryStream = new();
        switch (Path.GetExtension(sourcePath))
        {
            case ".ico":
                IcoToPngMemoryStream(sourcePath, memoryStream);
                break;
            
            case ".desktop":
                // TODO: implement the parsing of the .desktop file
                throw new NotImplementedException("*.desktop extension not supported.");
            
            case ".exe":
                if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    Icon? gameIcon = Icon.ExtractAssociatedIcon(sourcePath);
                    gameIcon?.ToBitmap().Save(memoryStream, ImageFormat.Png);
                }
                else if (OperatingSystem.IsLinux())
                {
                    string tempIcon = Path.Combine(appPaths.Temp, $"{Path.GetRandomFileName()}.ico");

                    try
                    {
                        // Extract icon using wrestool (icoutils package)
                        using Process process = new();
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = "wrestool",
                            Arguments = $"-x -o \"{tempIcon}\" -t 14 \"{sourcePath}\"",
                            WorkingDirectory = appPaths.Temp,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };

                        process.Start();
                        await process.WaitForExitAsync();

                        // TODO: log errors
                        /*using (StreamReader reader = process.StandardOutput)
                        {
                            string stderr =
                                process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                            string
                                result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                        }

                        using (StreamReader reader = process.StandardError)
                        {
                            string stderr =
                                process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                            string
                                result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                        }*/

                        if (File.Exists(tempIcon))
                        {
                            IcoToPngMemoryStream(tempIcon, memoryStream);
                        }
                    }
                    finally
                    {
                        File.Delete(tempIcon);
                    }
                }
                break;
        }

        await File.WriteAllBytesAsync(outputPath, memoryStream.GetBuffer());
    }

    private static void IcoToPngMemoryStream(string output, MemoryStream memoryStream)
    {
        using var fileStream = new FileStream(output, FileMode.Open);

        using var iconCodec = SKCodec.Create(fileStream);
        using var bitmap = SKBitmap.Decode(iconCodec);
        using var image = SKImage.FromBitmap(bitmap);
        using var pngData = image.Encode(SKEncodedImageFormat.Png, 100);
        pngData.SaveTo(memoryStream);
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