namespace PCGamingModApp.Core.Messaging.Messages;

public class GameDeletedMessage(Guid gameId)
{
    public Guid GameId { get; } = gameId;
}