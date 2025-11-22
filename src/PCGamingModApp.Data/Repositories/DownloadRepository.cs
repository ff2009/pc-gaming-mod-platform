using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data.Repositories;

public class DownloadRepository(AppDbContext context) : IDownloadRepository
{
    public async Task AddDownload(DownloadDataModel download)
    {
        context.Downloads.Add(download);
        await context.SaveChangesAsync();
    }

    public async Task UpdateDownload(DownloadDataModel download)
    {
        var existingDownload = await context.Games.FindAsync(download.Id);
        if (existingDownload != null)
        {
            // Updates only the required fields
            context.Entry(existingDownload).CurrentValues.SetValues(download);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteDownload(Guid id)
    {
        var download = await context.Downloads.FindAsync(id);
        if (download != null)
        {
            context.Downloads.Remove(download);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<DownloadDataModel>> GetAllDownloads()
    {
        return await context.Downloads.ToListAsync();
    }

    public async Task<DownloadDataModel?> GetDownloadById(Guid id)
    {
        return await context.Downloads.FindAsync(id);
    }
}