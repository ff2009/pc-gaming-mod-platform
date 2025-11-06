using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

public class GameManagerService(IAppPaths appPaths, IDialogService dialogService, IGameRepository gameRepository)
{
    public (string? iconPath, string? gameTitle) ExtractLinuxGameInfo(string desktopFilePath)
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

    private async Task SaveGameIcon(string sourcePath, string iconName)
    {
        if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            Icon? gameIcon = Icon.ExtractAssociatedIcon(sourcePath);
            if (gameIcon is null)
                return;

            string iconPath = Path.Combine(appPaths.GameIcons, $"{iconName}.png");
            gameIcon.ToBitmap().Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
        }
        else if (OperatingSystem.IsLinux())
        {
            byte[] gameIcon = this.ExtractExeIconOnLinux(sourcePath);
            if (gameIcon?.Length == 0)
                return;

            await using FileStream fileStream = new(Path.Combine(appPaths.GameIcons, $"{iconName}.png"),
                FileMode.Create, FileAccess.Write);
            
            await fileStream.WriteAsync(gameIcon, 0, gameIcon.Length);

            // if (File.Exists(iconPath))
            // {
            //     string destPath = Path.Combine(appPaths.GameIcons, $"{iconName}.png");
            //     File.Copy(iconPath, destPath, overwrite: true);
            // }
        }
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

        string? result = await dialogService.FilePicker(options);


        return result;
    }

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

        await this.SaveGameIcon(gameExecutablePath, gameTitle);
        if (string.IsNullOrWhiteSpace(gameTitle))
        {
            // user cancelled or no valid selection
            return null;
        }

        // Convert the VM back to a domain object and hand it to the repo.
        GameDataModel newGame = new()
        {
            Name = gameTitle,
            IconKey = $"{gameTitle}.png",
            InstallPath = gameExecutablePath,
            IsInstalled = true
        };

        await gameRepository.AddGame(newGame);
        return newGame;
    }


    private byte[] ExtractExeIconOnLinux(string exePath)
    {
        byte[] iconData = null;

        try
        {
            using var fileStream = new FileStream(exePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fileStream);
            // Check if this is a valid PE file
            fileStream.Seek(0x3C, SeekOrigin.Begin);
            int peHeaderOffset = br.ReadInt32();
            fileStream.Seek(peHeaderOffset, SeekOrigin.Begin);
            uint peSignature = br.ReadUInt32();
            if (peSignature != 0x00004550) // "PE\0\0"
                throw new Exception("Not a valid PE file.");

            // Locate the resource section
            fileStream.Seek(peHeaderOffset + 0x18, SeekOrigin.Begin);
            ushort numSections = br.ReadUInt16();
            fileStream.Seek(peHeaderOffset + 0xF8, SeekOrigin.Begin); // Skip to section headers

            long resourceSectionOffset = 0;
            for (int i = 0; i < numSections; i++)
            {
                // Read section name (8 bytes)
                byte[] nameBytes = br.ReadBytes(8);
                string sectionName = Encoding.ASCII.GetString(nameBytes).Split('\0')[0];

                if (sectionName == ".rsrc")
                {
                    resourceSectionOffset = fileStream.Position - 8;
                    break;
                }

                // Skip to next section header (40 bytes total per section)
                fileStream.Seek(32, SeekOrigin.Current);
            }

            if (resourceSectionOffset == 0)
                throw new Exception("No resource section found.");

            // Parse the resource directory to find the icon
            // TODO: Implement full resource directory parsing for robustness
            // For now, skip to a likely icon location (simplified)
            fileStream.Seek(0x1000, SeekOrigin.Begin);
            iconData = br.ReadBytes(1024); // Read a chunk of data (simplified)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract icon: {ex.Message}");
        }

        return iconData;
    }
}