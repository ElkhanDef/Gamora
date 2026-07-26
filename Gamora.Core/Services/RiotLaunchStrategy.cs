using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class RiotLaunchStrategy : LaunchStrategyBase
{
    // Riot Client'ın standart kurulum yolu. Kafe imajında farklıysa ileride settings.json'a
    // taşınabilir; DEVELOPMENT.md şimdilik sabit yol varsayıyor.
    private const string RiotClientPath = @"C:\Riot Games\Riot Client\RiotClientServices.exe";

    public override LaunchType LaunchType => LaunchType.Riot;

    protected override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        return new LaunchCommand(RiotClientPath, $"--launch-product={game.LaunchTarget} --launch-patchline=live");
    }
}
