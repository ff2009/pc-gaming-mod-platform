using System;
using System.Collections.Concurrent;
using System.IO;
using Avalonia.Media.Imaging;
using PCGamingModApp.Services.Interfaces;

namespace PCGamingModApp.Services.Implementations;

public sealed class SimpleImageCache : IImageCache
{
    // Base folder where all icons live (e.g. "Assets/StoreIcons/")
    private readonly string _baseFolder;

    private readonly ConcurrentDictionary<string, Lazy<Bitmap?>> _cache =
        new ConcurrentDictionary<string, Lazy<Bitmap?>>();

    public SimpleImageCache(string baseFolder) => _baseFolder = baseFolder;

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