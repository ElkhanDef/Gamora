using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class StatsServiceTests : IDisposable
{
    private readonly string _tempDir;

    public StatsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GamoraStatsTests_" + Guid.NewGuid());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task RecordLaunchAsync_klasor_yoksa_olusturur_ve_dosyaya_ekler()
    {
        var sut = new StatsService("PC-TEST");

        await sut.RecordLaunchAsync(_tempDir, "cs2");

        var filePath = Path.Combine(_tempDir, "PC-TEST.jsonl");
        Assert.True(File.Exists(filePath));

        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains("\"gameId\":\"cs2\"", content);
        Assert.Contains("\"event\":\"launch\"", content);
    }

    [Fact]
    public async Task RecordLaunchAsync_arka_arkaya_cagrilar_ayni_dosyaya_satir_ekler()
    {
        var sut = new StatsService("PC-TEST");

        await sut.RecordLaunchAsync(_tempDir, "cs2");
        await sut.RecordLaunchAsync(_tempDir, "valorant");

        var filePath = Path.Combine(_tempDir, "PC-TEST.jsonl");
        var lines = await File.ReadAllLinesAsync(filePath);

        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public async Task RecordLaunchAsync_sadece_kendi_makine_dosyasina_yazar()
    {
        var sut = new StatsService("PC-01");

        await sut.RecordLaunchAsync(_tempDir, "cs2");

        Assert.True(File.Exists(Path.Combine(_tempDir, "PC-01.jsonl")));
        Assert.False(File.Exists(Path.Combine(_tempDir, "PC-02.jsonl")));
    }

    [Fact]
    public async Task RecordLaunchAsync_gecersiz_yol_hata_firlatmaz()
    {
        // Yol içinde NUL karakteri — Directory.CreateDirectory kesin başarısız olur,
        // ama RecordLaunchAsync yine de fırlatmamalı (istatistik kritik değil).
        var sut = new StatsService("PC-TEST");
        var invalidPath = _tempDir + "\0invalid";

        var exception = await Record.ExceptionAsync(() => sut.RecordLaunchAsync(invalidPath, "cs2"));

        Assert.Null(exception);
    }
}
