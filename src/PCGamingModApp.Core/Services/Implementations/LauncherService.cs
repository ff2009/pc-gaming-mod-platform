using PCGamingModApp.Core.Services.Interfaces;
using System.Diagnostics;

namespace PCGamingModApp.Core.Services.Implementations;

public sealed class LauncherService : ILauncherService
{
    public Task LaunchAsync(string executablePath, CancellationToken ct = default) =>
        Task.Run(() => Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true
        }), ct);
}