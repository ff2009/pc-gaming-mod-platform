using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PCGamingModApp.Services.Interfaces;

namespace PCGamingModApp.Services.Implementations;

public sealed class LauncherService : ILauncherService
{
    public Task LaunchAsync(string executablePath, CancellationToken ct = default) =>
        Task.Run(() => Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true
        }), ct);
}