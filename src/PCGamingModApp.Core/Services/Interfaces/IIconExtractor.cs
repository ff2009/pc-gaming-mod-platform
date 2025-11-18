namespace PCGamingModApp.Core.Services.Interfaces;

public interface IIconExtractor
{
    Task ExtractAndSaveAsync(string sourcePath, string outputPath);
}