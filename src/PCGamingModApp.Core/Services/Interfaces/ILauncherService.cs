namespace PCGamingModApp.Core.Services.Interfaces;

/// <summary>
/// ILauncherService – abstracts Process.Start
/// </summary>
public interface ILauncherService
{
    Task LaunchAsync(string executablePath, CancellationToken ct = default);
}