namespace PCGamingModApp.Core.Messaging.Messages;

public class FilterTextMessage(string filterText)
{
    public string FilterText { get; } = filterText;
}