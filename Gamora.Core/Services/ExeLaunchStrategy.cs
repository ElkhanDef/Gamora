using Gamora.Core.Abstractions;
using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class ExeLaunchStrategy(IPathResolver pathResolver) : LaunchStrategyBase
{
    public override LaunchType LaunchType => LaunchType.Exe;

    protected override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        var resolvedPath = pathResolver.Resolve(game.LaunchTarget, settings);
        return new LaunchCommand(resolvedPath, game.Args, game.WorkingDir);
    }
}
