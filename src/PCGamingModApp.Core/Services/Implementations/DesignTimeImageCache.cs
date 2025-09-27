using Avalonia.Media.Imaging;
using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Core.Services.Implementations;

public class DesignTimeImageCache(string baseFolder) : IImageCache
{
    /// <summary>
    /// Base folder where all icons live (e.g. "Assets/Images/")
    /// </summary>
    private readonly string _baseFolder = Path.IsPathRooted(baseFolder) 
        ? baseFolder 
        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseFolder);

    public Bitmap? GetBitmap(string key)
    {
        string fullPath = Path.Combine(_baseFolder, key);

        return new Bitmap(fullPath);
    }
}