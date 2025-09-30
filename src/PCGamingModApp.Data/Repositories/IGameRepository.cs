using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Data.Repositories;

public interface IGameRepository
{
    Task AddGame(GameDataModel game);
    Task UpdateGame(GameDataModel game);
    Task DeleteGame(Guid id);
    Task<List<GameDataModel>> GetAllGames();
    Task<GameDataModel?> GetGameById(Guid id);
}