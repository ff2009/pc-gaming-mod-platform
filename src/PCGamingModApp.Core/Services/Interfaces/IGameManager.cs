using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IGameManager
{
    /// <summary>
    /// Adds games to the library
    /// </summary>
    /// <param name="gameExecutablePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Task<GameDataModel?> AddGame(string gameExecutablePath);

    /// <summary>
    /// Selects game executable
    /// </summary>
    /// <returns></returns>
    public Task<string?> SelectGameExecutableAsync();
}