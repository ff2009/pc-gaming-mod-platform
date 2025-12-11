using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using PCGamingModApp.Core.Services.Interfaces;
using SkiaSharp;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class IconExtractorService(IAppPaths appPaths) : IIconExtractor
{
    public async Task ExtractAndSaveAsync(string sourcePath, string outputPath)
    {
        await using var memoryStream = new MemoryStream();
        switch (Path.GetExtension(sourcePath))
        {
            case ".ico":
                IcoToPngMemoryStream(sourcePath, memoryStream);
                break;

            case ".desktop":
                // TODO: implement the parsing of the .desktop file
                throw new Exception($"Unsupported file type: {Path.GetExtension(sourcePath)}");

            case ".exe":
                if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    using var icon = Icon.ExtractAssociatedIcon(sourcePath);
                    icon?.ToBitmap().Save(memoryStream, ImageFormat.Png);
                }
                else
                {
                    // Linux: Use wrestool (abstracted)
                    await ExtractIconWithWrestoolAsync(sourcePath, memoryStream);
                }

                break;

            default:
                throw new Exception($"Unsupported file type: {Path.GetExtension(sourcePath)}");
        }

        await File.WriteAllBytesAsync(outputPath, memoryStream.ToArray());
    }

    private void IcoToPngMemoryStream(string output, MemoryStream memoryStream)
    {
        using var fileStream = new FileStream(output, FileMode.Open);

        using var iconCodec = SKCodec.Create(fileStream);
        using var bitmap = SKBitmap.Decode(iconCodec);
        using var image = SKImage.FromBitmap(bitmap);
        using var pngData = image.Encode(SKEncodedImageFormat.Png, 100);
        pngData.SaveTo(memoryStream);
    }

    private async Task ExtractIconWithWrestoolAsync(string sourcePath, MemoryStream memoryStream)
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
}