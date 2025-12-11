using System.Drawing;
using System.Drawing.Imaging;
using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class GameIconService(IAppPaths appPaths, IIconExtractor iconExtractor) : IGameIconService
{
    public async Task<string?> SaveIconAsync(string gameTitle, string sourcePath)
    {
        string iconFilename = $"{gameTitle.Trim(Path.GetInvalidFileNameChars()).ToLowerInvariant().Replace(" ", "-")}-{Guid.NewGuid():N}.png";
        string outputPath = Path.Combine(appPaths.GameIcons, iconFilename);

        await iconExtractor.ExtractAndSaveAsync(sourcePath, outputPath);
        return outputPath;
    }
}