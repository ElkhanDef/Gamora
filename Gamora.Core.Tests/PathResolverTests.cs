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

    [Fact]
    public void ToTemplate_oyun_diski_altindaki_yolu_GAMEDISK_yapar()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"G:\Gamora" };

        var result = resolver.ToTemplate(@"G:\Gamora\Games\Oyun\oyun.exe", settings);

        Assert.Equal(@"{GAMEDISK}\Games\Oyun\oyun.exe", result);
    }

    [Fact]
    public void ToTemplate_oyun_diski_disindaki_yolu_degistirmeden_dondurur()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"G:\Gamora" };

        var result = resolver.ToTemplate(@"C:\SabitYol\oyun.exe", settings);

        Assert.Equal(@"C:\SabitYol\oyun.exe", result);
    }

    // Regresyon: settings.json elle düzenlenip "/" ile yazılmışsa (ör. "C:/GamoraData"),
    // OpenFileDialog'un döndürdüğü "\" tabanlı gerçek yolla eşleşmesi gerekiyordu ama
    // StartsWith büyük/küçük harfi yok sayıp ayracı yok saymadığı için hiç eşleşmiyordu.
    [Fact]
    public void ToTemplate_ayarda_ileri_egik_cizgi_olsa_da_ters_egik_cizgili_yolu_eslestirir()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = "C:/GamoraData" };

        var result = resolver.ToTemplate(@"C:\GamoraData\TS4_x64.exe", settings);

        Assert.Equal(@"{GAMEDISK}\TS4_x64.exe", result);
    }

    [Fact]
    public void ToTemplate_ayarda_sonda_egik_cizgi_olsa_da_dogru_eslestirir()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"G:\Gamora\" };

        var result = resolver.ToTemplate(@"G:\Gamora\Games\Oyun\oyun.exe", settings);

        Assert.Equal(@"{GAMEDISK}\Games\Oyun\oyun.exe", result);
    }

    [Fact]
    public void ToTemplate_buyuk_kucuk_harf_farkini_yok_sayar()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"G:\Gamora" };

        var result = resolver.ToTemplate(@"g:\gamora\Games\oyun.exe", settings);

        Assert.Equal(@"{GAMEDISK}\Games\oyun.exe", result);
    }

    // Kafede gameDisk çoğu zaman salt bir sürücü harfi olacak (ör. "G:"), klasör değil.
    [Fact]
    public void ToTemplate_gameDisk_salt_surucu_harfiyken_calisir()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = "G:" };

        var result = resolver.ToTemplate(@"G:\Games\Oyun\oyun.exe", settings);

        Assert.Equal(@"{GAMEDISK}\Games\Oyun\oyun.exe", result);
    }

    // Regresyon: eski StartsWith mantığı salt önek kontrolü yaptığı için "C:\GamoraData" ile
    // "C:\GamoraDataOther\..." gibi komşu bir klasörü yanlışlıkla oyun diskinin İÇİ sanıyordu.
    [Fact]
    public void ToTemplate_benzer_onekli_komsu_klasoru_esles_saymaz()
    {
        var resolver = new PathResolver();
        var settings = new LauncherSettings { GameDisk = @"C:\GamoraData" };

        var result = resolver.ToTemplate(@"C:\GamoraDataOther\oyun.exe", settings);

        Assert.Equal(@"C:\GamoraDataOther\oyun.exe", result);
    }
}
