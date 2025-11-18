namespace PCGamingModApp.Core.Services.Interfaces;

public interface IGameIconService
{
    Task<string?> SaveIconAsync(string gameTitle, string sourcePath);
}