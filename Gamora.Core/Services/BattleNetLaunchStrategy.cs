using Gamora.Core.Models;

namespace Gamora.Core.Services;

// DEVELOPMENT.md battlenet/epic için tam kalıp vermiyor ("kendi URI/komut kalıpları").
// battlenet:// gerçek, kayıtlı bir URI şeması; launchTarget içinde admin'in oyuna göre
// dolduracağı ürün kodu (ör. "OW", "WoW") bekleniyor. Gerçek cihazda doğrulanmalı.
public sealed class BattleNetLaunchStrategy : LaunchStrategyBase
{
    public override LaunchType LaunchType => LaunchType.Battlenet;

    internal override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        // Ürün kodu girilmemişse çıplak battlenet:// istemciyi açar (ürün kodu olmadan) —
        // Battle.net masaüstü uygulaması gelir, müşteri oyunu oradan başlatır.
        if (string.IsNullOrWhiteSpace(game.LaunchTarget))
        {
            return new LaunchCommand("battlenet://", UseShellExecute: true, IsPlatformFallback: true, FallbackPlatformLabel: "Battle.net");
        }

        return new LaunchCommand($"battlenet://{game.LaunchTarget}", UseShellExecute: true);
    }
}
