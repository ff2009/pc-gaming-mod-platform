using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;
using System.Diagnostics;
using System.Drawing;

namespace PCGamingModApp.Core.Services.Implementations;

public class GameManagerService(IAppPaths _appPaths, IDialogService _dialogService)
{
    public (Icon, string) ExtractWindowsGameInfo(string exePath)
    {
        // Extract icon
        Icon gameIcon = Icon.ExtractAssociatedIcon(exePath);

        // Extract game title (ProductName from version info)
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(exePath);
        string gameTitle = !string.IsNullOrEmpty(versionInfo.ProductName)
            ? versionInfo.ProductName
            : Path.GetFileNameWithoutExtension(exePath);

        return (gameIcon, gameTitle);
    }
    
    public (string IconPath, string GameTitle) ExtractLinuxGameInfo(string desktopFilePath)
    {
        // Parse .desktop file (simplified example)
        var lines = File.ReadAllLines(desktopFilePath);
        string iconPath = null;
        string gameTitle = null;

        foreach (var line in lines)
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
            string[] iconDirs = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "icons"),
                "/usr/share/icons"
            };

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
    
    public string? SaveGameIcon(string sourcePath)
    {
        string output = null;
        if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            var (icon, gameTitle) = ExtractWindowsGameInfo(sourcePath);
            string iconPath = Path.Combine(_appPaths.GameIcons, $"{gameTitle}.png");
            icon.ToBitmap().Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
            output = gameTitle;
        }
        else if (OperatingSystem.IsLinux())
        {
            var (iconPath, gameTitle) = ExtractLinuxGameInfo(sourcePath);
            if (File.Exists(iconPath))
            {
                string destPath = Path.Combine(_appPaths.GameIcons, $"{gameTitle}.png");
                File.Copy(iconPath, destPath, overwrite: true);
            }
            output = gameTitle;
        }

        return output;
    }
    
    public async Task<string?> SelectGameExecutableAsync()
    {
        FilePickerOpenOptions options = new()
        {
            Title = "Select Game Executable",
            FileTypeFilter =
            [
                new FilePickerFileType("Executables")
                {
                    Patterns = ["*.exe", "*.bin", "*.app"]
                }
            ],
            AllowMultiple = false
        };
        
        string? result = await _dialogService.FilePicker(options);
        
        
        
        return result;
    }
}