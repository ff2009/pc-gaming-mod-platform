using Avalonia.Media.Imaging;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IImageCache
{
    /// <summary>
    /// Returns a cached bitmap for the supplied key, loading it lazily if needed.
    /// Returns null if the key cannot be resolved.
    /// </summary>
    Bitmap? GetBitmap(string key);
}