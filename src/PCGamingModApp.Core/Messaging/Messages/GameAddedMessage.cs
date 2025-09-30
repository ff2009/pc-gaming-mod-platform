using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Messaging.Messages;

public class GameAddedMessage(GameDataModel game)
{
    public GameDataModel Game { get; } = game;
}