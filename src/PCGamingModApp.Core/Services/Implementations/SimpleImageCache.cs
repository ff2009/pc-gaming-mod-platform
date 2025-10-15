using Avalonia.Media.Imaging;
using PCGamingModApp.Core.Services.Interfaces;
using System.Collections.Concurrent;

namespace PCGamingModApp.Core.Services.Implementations;

public sealed class SimpleImageCache(string baseFolder) : IImageCache
{
    /// <summary>
    /// Base folder where all icons live   
    /// </summary>
    private readonly string _baseFolder = Path.IsPathRooted(baseFolder) ?
        baseFolder : 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseFolder);


    private readonly ConcurrentDictionary<string, Lazy<Bitmap?>> _cache = new();

    public Bitmap? GetBitmap(string key)
    {
        // The Lazy ensures the image is loaded only once, even if accessed concurrently.
        var lazyBmp = _cache.GetOrAdd(key, k => new Lazy<Bitmap?>(LoadBitmap));
        return lazyBmp.Value;

        Bitmap? LoadBitmap()
        {
            var fullPath = Path.Combine(_baseFolder, key);
            if (!File.Exists(fullPath)) return null;

            // Avalonia can load from a stream; this keeps the file unlocked.
            using var stream = File.OpenRead(fullPath);
            return Bitmap.DecodeToWidth(stream, 32); // resize to a reasonable thumbnail size
        }
    }
}