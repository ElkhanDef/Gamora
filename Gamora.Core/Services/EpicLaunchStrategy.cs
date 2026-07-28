using Gamora.Core.Models;

namespace Gamora.Core.Services;

// DEVELOPMENT.md battlenet/epic için tam kalıp vermiyor ("kendi URI/komut kalıpları").
// com.epicgames.launcher:// Epic'in bilinen URI şeması; launchTarget içinde admin'in
// dolduracağı app namespace/ID bekleniyor. Gerçek cihazda doğrulanmalı.
public sealed class EpicLaunchStrategy : LaunchStrategyBase
{
    public override LaunchType LaunchType => LaunchType.Epic;

    internal override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        // Uygulama kodu girilmemişse çıplak com.epicgames.launcher:// açar — Epic Games Launcher
        // gelir, müşteri oyunu kütüphaneden kendisi başlatır.
        if (string.IsNullOrWhiteSpace(game.LaunchTarget))
        {
            return new LaunchCommand("com.epicgames.launcher://", UseShellExecute: true, IsPlatformFallback: true, FallbackPlatformLabel: "Epic Games");
        }

        return new LaunchCommand($"com.epicgames.launcher://apps/{game.LaunchTarget}?action=launch&silent=true", UseShellExecute: true);
    }
}
