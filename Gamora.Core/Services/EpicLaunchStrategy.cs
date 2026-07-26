using Gamora.Core.Models;

namespace Gamora.Core.Services;

// DEVELOPMENT.md battlenet/epic için tam kalıp vermiyor ("kendi URI/komut kalıpları").
// com.epicgames.launcher:// Epic'in bilinen URI şeması; launchTarget içinde admin'in
// dolduracağı app namespace/ID bekleniyor. Gerçek cihazda doğrulanmalı.
public sealed class EpicLaunchStrategy : LaunchStrategyBase
{
    public override LaunchType LaunchType => LaunchType.Epic;

    protected override LaunchCommand BuildCommand(Game game, LauncherSettings settings)
    {
        return new LaunchCommand($"com.epicgames.launcher://apps/{game.LaunchTarget}?action=launch&silent=true", UseShellExecute: true);
    }
}
