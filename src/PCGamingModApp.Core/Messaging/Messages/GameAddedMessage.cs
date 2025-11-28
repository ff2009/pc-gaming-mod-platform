using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Messaging.Messages;

public class GameAddedMessage(GameDataModel game)
{
    public GameDataModel Game { get; } = game;
}

public class GameUpdatedMessage(GameDataModel game)
{
    public GameDataModel Game { get; } = game;
}

public class GameDeletedMessage(Guid gameId)
{
    public Guid GameId { get; } = gameId;
}