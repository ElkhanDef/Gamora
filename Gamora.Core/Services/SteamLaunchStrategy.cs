using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class SteamLaunchStrategy : LaunchStrategyBase
{
    public override LaunchType LaunchType => LaunchType.Steam;

    internal override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        // AppID girilmemişse (admin "kodu bilmiyorum" işaretlediyse) belirli bir oyunu değil,
        // Steam istemcisinin ana penceresini açarız — müşteri oyunu kütüphaneden kendisi başlatır.
        // steam://open/main resmi/belgeli bir Steam URI'si.
        if (string.IsNullOrWhiteSpace(game.LaunchTarget))
        {
            return new LaunchCommand("steam://open/main", UseShellExecute: true, IsPlatformFallback: true, FallbackPlatformLabel: "Steam");
        }

        return new LaunchCommand($"steam://rungameid/{game.LaunchTarget}", UseShellExecute: true);
    }
}
