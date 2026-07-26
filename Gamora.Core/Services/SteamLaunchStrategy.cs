using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class SteamLaunchStrategy : LaunchStrategyBase
{
    public override LaunchType LaunchType => LaunchType.Steam;

    protected override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        return new LaunchCommand($"steam://rungameid/{game.LaunchTarget}", UseShellExecute: true);
    }
}
