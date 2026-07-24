using Gamora.Core.Models;
using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class CatalogServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly CatalogService _sut = new();

    public CatalogServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GamoraCatalogTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_sonra_LoadAsync_ayni_veriyi_dondurur()
    {
        var catalogPath = Path.Combine(_tempDir, "catalog.json");
        var catalog = new Catalog
        {
            Version = 1,
            UpdatedAt = new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
            Categories = ["FPS", "MOBA"],
            Games =
            [
                new Game
                {
                    Id = "cs2",
                    Name = "Counter-Strike 2",
                    Category = "FPS",
                    LaunchType = LaunchType.Steam,
                    LaunchTarget = "730",
                    Visible = true,
                    SortOrder = 1,
                    AgeRestricted = true
                }
            ]
        };

        await _sut.SaveAsync(catalog, catalogPath);
        var loaded = await _sut.LoadAsync(catalogPath);

        Assert.Equal(catalog.Version, loaded.Version);
        Assert.Single(loaded.Games);
        Assert.Equal("cs2", loaded.Games[0].Id);
        Assert.Equal(LaunchType.Steam, loaded.Games[0].LaunchType);
        Assert.Equal("730", loaded.Games[0].LaunchTarget);
    }

    [Fact]
    public async Task SaveAsync_launchType_degerini_kucuk_harfle_json_e_yazar()
    {
        var catalogPath = Path.Combine(_tempDir, "catalog.json");
        var catalog = new Catalog
        {
            Games = [new Game { Id = "ow2", Name = "Overwatch 2", Category = "FPS", LaunchType = LaunchType.Battlenet, LaunchTarget = "x" }]
        };

        await _sut.SaveAsync(catalog, catalogPath);
        var json = await File.ReadAllTextAsync(catalogPath);

        Assert.Contains("\"launchType\": \"battlenet\"", json);
    }

    [Fact]
    public async Task SaveAsync_gecici_tmp_dosyasini_silip_gercek_dosyayi_birakir()
    {
        var catalogPath = Path.Combine(_tempDir, "catalog.json");

        await _sut.SaveAsync(new Catalog(), catalogPath);

        Assert.True(File.Exists(catalogPath));
        Assert.False(File.Exists(catalogPath + ".tmp"));
    }

    [Fact]
    public async Task SaveAsync_var_olan_dosyanin_uzerine_atomik_olarak_yazar()
    {
        var catalogPath = Path.Combine(_tempDir, "catalog.json");
        await _sut.SaveAsync(new Catalog { Version = 1 }, catalogPath);
        await _sut.SaveAsync(new Catalog { Version = 2 }, catalogPath);

        var loaded = await _sut.LoadAsync(catalogPath);
        Assert.Equal(2, loaded.Version);
    }
}
