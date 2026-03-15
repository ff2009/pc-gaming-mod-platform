using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data.Repositories;

public class DownloadSettingsRepository(AppDbContext dbContext) : IDownloadSettingsRepository
{
    public async Task<DownloadSettings> GetSettingsAsync()
    {
        var settings = await dbContext.DownloadSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new DownloadSettings();
            await dbContext.DownloadSettings.AddAsync(settings);
            await dbContext.SaveChangesAsync();
        }
        
        return settings;
    }
    
    public async Task UpdateSettingsAsync(DownloadSettings settings)
    {
        dbContext.DownloadSettings.Update(settings);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task InitializeDefaultSettingsAsync()
    {
        var existingSettings = await dbContext.DownloadSettings.FirstOrDefaultAsync();
        if (existingSettings == null)
        {
            var defaultSettings = new DownloadSettings();
            await dbContext.DownloadSettings.AddAsync(defaultSettings);
            await dbContext.SaveChangesAsync();
        }
    }
}