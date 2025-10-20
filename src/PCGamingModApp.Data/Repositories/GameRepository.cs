using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data.Repositories;

public class GameRepository(AppDbContext context) : IGameRepository
{
    public async Task AddGame(GameDataModel game)
    {
        context.Games.Add(game);
        await context.SaveChangesAsync();
    }

    public async Task UpdateGame(GameDataModel game)
    {
        var existingGame = await context.Games.FindAsync(game.Id);
        if (existingGame != null)
        {
            // Updates only the required fields
            context.Entry(existingGame).CurrentValues.SetValues(game);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteGame(Guid id)
    {
        var game = await context.Games.FindAsync(id);
        if (game != null)
        {
            context.Games.Remove(game);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<GameDataModel>> GetAllGames()
    {
        return await context.Games.ToListAsync();
    }

    public async Task<GameDataModel?> GetGameById(Guid id)
    {
        return await context.Games.FindAsync(id);
    }
}