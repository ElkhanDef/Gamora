using Gamora.Core.Models;
using Gamora.Core.Services;

namespace Gamora.Core.Tests;

// BuildCommand internal (bkz. AssemblyInfo.cs'teki InternalsVisibleTo) — burada gerçek bir
// process başlatmadan (Process.Start yalnızca LaunchAsync'te, TestMode dışında çalışır) hangi
// komutun üretileceğini doğrudan doğruluyoruz.
public class LaunchStrategyFallbackTests
{
    private static readonly LauncherSettings Settings = new();

    private static Game GameWithTarget(LaunchType type, string target) => new()
    {
        Id = "test",
        Name = "Test Oyunu",
        Category = "Test",
        LaunchType = type,
        LaunchTarget = target
    };

    [Fact]
    public void Steam_hedef_varsa_oyunu_acar()
    {
        var command = new SteamLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Steam, "730"), Settings);

        Assert.Equal("steam://rungameid/730", command.FileName);
        Assert.False(command.IsPlatformFallback);
    }

    [Fact]
    public void Steam_hedef_bossa_steam_istemcisini_acar()
    {
        var command = new SteamLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Steam, ""), Settings);

        Assert.Equal("steam://open/main", command.FileName);
        Assert.True(command.IsPlatformFallback);
        Assert.Equal("Steam", command.FallbackPlatformLabel);
    }

    [Fact]
    public void Riot_hedef_varsa_urun_koduyla_acilir()
    {
        var command = new RiotLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Riot, "valorant"), Settings);

        Assert.Contains("--launch-product=valorant", command.Arguments);
        Assert.False(command.IsPlatformFallback);
    }

    [Fact]
    public void Riot_hedef_bosluktan_ibaretse_client_argumansiz_acilir()
    {
        var command = new RiotLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Riot, "   "), Settings);

        Assert.Null(command.Arguments);
        Assert.True(command.IsPlatformFallback);
        Assert.Equal("Riot Client", command.FallbackPlatformLabel);
    }

    [Fact]
    public void Battlenet_hedef_varsa_urun_koduyla_acilir()
    {
        var command = new BattleNetLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Battlenet, "OW"), Settings);

        Assert.Equal("battlenet://OW", command.FileName);
        Assert.False(command.IsPlatformFallback);
    }

    [Fact]
    public void Battlenet_hedef_bossa_ciplak_uri_ile_client_acilir()
    {
        var command = new BattleNetLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Battlenet, ""), Settings);

        Assert.Equal("battlenet://", command.FileName);
        Assert.True(command.IsPlatformFallback);
        Assert.Equal("Battle.net", command.FallbackPlatformLabel);
    }

    [Fact]
    public void Epic_hedef_varsa_uygulama_koduyla_acilir()
    {
        var command = new EpicLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Epic, "abc123"), Settings);

        Assert.Equal("com.epicgames.launcher://apps/abc123?action=launch&silent=true", command.FileName);
        Assert.False(command.IsPlatformFallback);
    }

    [Fact]
    public void Epic_hedef_bossa_ciplak_uri_ile_launcher_acilir()
    {
        var command = new EpicLaunchStrategy().BuildCommand(GameWithTarget(LaunchType.Epic, ""), Settings);

        Assert.Equal("com.epicgames.launcher://", command.FileName);
        Assert.True(command.IsPlatformFallback);
        Assert.Equal("Epic Games", command.FallbackPlatformLabel);
    }
}
