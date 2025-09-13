using System.Threading;
using System.Threading.Tasks;

namespace PCGamingModApp.Services.Interfaces;

/// <summary>
/// ILauncherService – abstracts Process.Start
/// </summary>
public interface ILauncherService
{
    Task LaunchAsync(string executablePath, CancellationToken ct = default);
}