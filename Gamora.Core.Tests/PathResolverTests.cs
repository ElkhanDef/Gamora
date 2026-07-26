using Gamora.Core.Models;
using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class PathResolverTests
{
    [Fact]
    public void Resolve_GAMEDISK_yerine_ayar_diskini_koyar()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"G:\Gamora" };

        var result = resolver.Resolve(@"{GAMEDISK}\Games\Oyun\oyun.exe", settings);

        Assert.Equal(@"G:\Gamora\Games\Oyun\oyun.exe", result);
    }

    [Fact]
    public void Resolve_GAMEDISK_gecmeyen_yolu_degistirmeden_dondurur()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"G:\Gamora" };

        var result = resolver.Resolve(@"C:\SabitYol\oyun.exe", settings);

        Assert.Equal(@"C:\SabitYol\oyun.exe", result);
    }
}
