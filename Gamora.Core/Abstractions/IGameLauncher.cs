using Gamora.Core.Models;

namespace Gamora.Core.Abstractions;

public interface IGameLauncher
{
    Task<LaunchResult> LaunchAsync(Game game, CancellationToken cancellationToken = default);
}
