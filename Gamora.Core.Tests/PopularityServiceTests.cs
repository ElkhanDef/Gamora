using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class PopularityServiceTests : IDisposable
{
    private readonly string _tempDir;

    public PopularityServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GamoraPopularityTests_" + Guid.NewGuid());
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
    public async Task LoadAsync_stats_klasoru_yoksa_sifir_dondurur()
    {
        var sut = new PopularityService();

        await sut.LoadAsync(Path.Combine(_tempDir, "yok"));

        Assert.Equal(0, sut.GetLaunchCount("cs2"));
    }

    [Fact]
    public async Task LoadAsync_birden_fazla_makine_dosyasini_toplar()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "PC-01.jsonl"),
            """
            {"gameId":"cs2","event":"launch","time":"2026-07-27T10:00:00"}
            {"gameId":"cs2","event":"launch","time":"2026-07-27T10:05:00"}
            {"gameId":"valorant","event":"launch","time":"2026-07-27T10:10:00"}
            """);
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "PC-02.jsonl"),
            """
            {"gameId":"cs2","event":"launch","time":"2026-07-27T11:00:00"}
            """);

        var sut = new PopularityService();
        await sut.LoadAsync(_tempDir);

        Assert.Equal(3, sut.GetLaunchCount("cs2"));
        Assert.Equal(1, sut.GetLaunchCount("valorant"));
        Assert.Equal(0, sut.GetLaunchCount("bilinmeyen-oyun"));
    }

    [Fact]
    public async Task LoadAsync_bozuk_satirlari_sessizce_atlar()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "PC-01.jsonl"),
            """
            {"gameId":"cs2","event":"launch","time":"2026-07-27T10:00:00"}
            {"gameId":"valorant","event":"lau
            {"gameId":"cs2","event":"launch","time":"2026-07-27T10:05:00"}

            """);

        var sut = new PopularityService();
        await sut.LoadAsync(_tempDir);

        Assert.Equal(2, sut.GetLaunchCount("cs2"));
        Assert.Equal(0, sut.GetLaunchCount("valorant"));
    }
}
