using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;
using PCGamingModApp.Data.Repositories;
using Xunit;

namespace PCGamingModApp.Tests.Integration.Repositories;

public class GameRepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IGameRepository _gameRepository;

    public GameRepositoryIntegrationTests()
    {
        // Configure the InMemory database with a unique name for each test run
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _gameRepository = new GameRepository(_context);
    }

    [Fact]
    public async Task AddGame_ShouldAddGameToDatabase()
    {
        // Arrange
        var game = new GameDataModel { Id = Guid.NewGuid(), Name = "Test Game", Store = StoreType.EaApp };

        // Act
        await _gameRepository.AddGame(game);

        // Assert
        var games = await _gameRepository.GetAllGames();
        Assert.NotNull(games);

        var loadedGame = games.Single();
        Assert.Equivalent(loadedGame.Name, "Test Game");
        Assert.Equivalent(loadedGame.Store, StoreType.EaApp);
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnEmptyList_WhenNoGamesExist()
    {
        // Act
        var games = await _gameRepository.GetAllGames();

        // Assert
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnAllGames_WhenGamesExist()
    {
        // Arrange
        var game1 = new GameDataModel() { Id = Guid.NewGuid(), Name = "Game 1" };
        var game2 = new GameDataModel() { Id = Guid.NewGuid(), Name = "Game 2" };
        await _gameRepository.AddGame(game1);
        await _gameRepository.AddGame(game2);

        // Act
        var games = await _gameRepository.GetAllGames();

        // Assert
        Assert.Equal(2, games.Count);
        Assert.Contains(games, g => g.Name == "Game 1");
        Assert.Contains(games, g => g.Name == "Game 2");
    }

    [Fact]
    public async Task DeleteGame_ShouldRemoveGameFromDatabase()
    {
        // Arrange
        var game = new GameDataModel { Id = Guid.NewGuid(), Name = "Game to Delete" };
        await _gameRepository.AddGame(game);

        // Act
        await _gameRepository.DeleteGame(game.Id);

        // Assert
        var games = await _gameRepository.GetAllGames();
        Assert.Empty(games);
    }

    [Fact]
    public async Task UpdateGame_ShouldModifyGameInDatabase()
    {
        // Arrange
        var game = new GameDataModel { Id = Guid.NewGuid(), Name = "Original Name" };
        await _gameRepository.AddGame(game);

        // Act
        game.Name = "Updated Name";
        await _gameRepository.UpdateGame(game);

        // Assert
        var updatedGame = (await _gameRepository.GetAllGames()).First();
        Assert.Equal("Updated Name", updatedGame.Name);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}