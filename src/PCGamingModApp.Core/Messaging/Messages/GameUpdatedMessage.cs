using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Messaging.Messages;

public class GameUpdatedMessage(GameDataModel game)
{
    public GameDataModel Game { get; } = game;
}