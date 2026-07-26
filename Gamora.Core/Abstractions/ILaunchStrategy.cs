using Gamora.Core.Models;

namespace Gamora.Core.Abstractions;

public interface ILaunchStrategy
{
    LaunchType LaunchType { get; }

    Task<LaunchResult> LaunchAsync(Game game, LauncherSettings settings, CancellationToken cancellationToken = default);
}
