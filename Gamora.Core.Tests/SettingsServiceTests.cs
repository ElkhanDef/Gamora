using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsPath;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GamoraSettingsTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _settingsPath = Path.Combine(_tempDir, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_dosya_yoksa_varsayilanlarla_olusturur()
    {
        var sut = new SettingsService(_settingsPath);

        var settings = await sut.LoadAsync();

        Assert.True(File.Exists(_settingsPath));
        Assert.Equal(@"C:\GamoraData", settings.GameDisk);
        Assert.Equal(@"C:\GamoraData", settings.DataRoot);
        Assert.True(settings.TestMode);
    }

    [Fact]
    public async Task LoadAsync_var_olan_dosyayi_okur()
    {
        await File.WriteAllTextAsync(_settingsPath, """{"gameDisk":"G:\\Gamora","dataRoot":"G:\\Gamora","testMode":false}""");
        var sut = new SettingsService(_settingsPath);

        var settings = await sut.LoadAsync();

        Assert.Equal(@"G:\Gamora", settings.GameDisk);
        Assert.Equal(@"G:\Gamora", settings.DataRoot);
        Assert.False(settings.TestMode);
    }

    [Fact]
    public async Task LoadAsync_gameDisk_ve_dataRoot_farkli_ayarlanabilir()
    {
        await File.WriteAllTextAsync(_settingsPath, """{"gameDisk":"G:\\","dataRoot":"G:\\Gamora","testMode":false}""");
        var sut = new SettingsService(_settingsPath);

        var settings = await sut.LoadAsync();

        Assert.Equal(@"G:\", settings.GameDisk);
        Assert.Equal(@"G:\Gamora", settings.DataRoot);
    }

    [Fact]
    public async Task CatalogPath_CoversPath_StatsPath_dataRoot_kokunden_turer()
    {
        var sut = new SettingsService(_settingsPath);

        var settings = await sut.LoadAsync();

        Assert.Equal(Path.Combine(settings.DataRoot, "catalog.json"), settings.CatalogPath);
        Assert.Equal(Path.Combine(settings.DataRoot, "covers"), settings.CoversPath);
        Assert.Equal(Path.Combine(settings.DataRoot, "stats"), settings.StatsPath);
    }

    [Fact]
    public async Task Turetilen_yollar_settings_json_a_yazilmaz()
    {
        var sut = new SettingsService(_settingsPath);
        await sut.LoadAsync();

        var json = await File.ReadAllTextAsync(_settingsPath);

        Assert.DoesNotContain("catalogPath", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("coversPath", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("statsPath", json, StringComparison.OrdinalIgnoreCase);
    }
}
