using System.Text.Json;
using Gamora.Core.Models;
using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class PasswordServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _lockPath;

    public PasswordServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GamoraPasswordTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _lockPath = Path.Combine(_tempDir, "sfr.lock");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task SetPasswordAsync_sonra_dogru_sifre_dogrulanir()
    {
        var sut = new PasswordService();

        var result = await sut.SetPasswordAsync(_lockPath, "gizli123");

        Assert.True(result.Success);
        Assert.True(await sut.VerifyPasswordAsync(_lockPath, "gizli123"));
    }

    [Fact]
    public async Task VerifyPasswordAsync_yanlis_sifre_reddedilir()
    {
        var sut = new PasswordService();
        await sut.SetPasswordAsync(_lockPath, "gizli123");

        Assert.False(await sut.VerifyPasswordAsync(_lockPath, "yanlis-sifre"));
    }

    [Fact]
    public async Task SetPasswordAsync_her_kurulumda_farkli_salt_uretir()
    {
        var sut = new PasswordService();
        var path2 = Path.Combine(_tempDir, "sfr2.lock");

        await sut.SetPasswordAsync(_lockPath, "ayni-sifre");
        await sut.SetPasswordAsync(path2, "ayni-sifre");

        var lock1 = JsonSerializer.Deserialize<AdminLock>(await File.ReadAllTextAsync(_lockPath), JsonOptions);
        var lock2 = JsonSerializer.Deserialize<AdminLock>(await File.ReadAllTextAsync(path2), JsonOptions);

        Assert.NotEqual(lock1!.Salt, lock2!.Salt);
        Assert.NotEqual(lock1.Hash, lock2.Hash);
    }

    [Fact]
    public async Task SetPasswordAsync_en_az_100000_iterasyon_kullanir()
    {
        var sut = new PasswordService();
        await sut.SetPasswordAsync(_lockPath, "gizli123");

        var lockData = JsonSerializer.Deserialize<AdminLock>(await File.ReadAllTextAsync(_lockPath), JsonOptions);

        Assert.True(lockData!.Iterations >= 100_000);
    }

    [Fact]
    public async Task SetPasswordAsync_duz_metin_veya_zayif_hash_icermez()
    {
        var sut = new PasswordService();
        await sut.SetPasswordAsync(_lockPath, "gizli123");

        var json = await File.ReadAllTextAsync(_lockPath);

        Assert.DoesNotContain("gizli123", json);
    }

    [Fact]
    public async Task SetPasswordAsync_var_olan_kilidin_uzerine_yazmaz()
    {
        var sut = new PasswordService();
        await sut.SetPasswordAsync(_lockPath, "ilk-sifre");

        var result = await sut.SetPasswordAsync(_lockPath, "ikinci-sifre");

        Assert.False(result.Success);
        Assert.True(await sut.VerifyPasswordAsync(_lockPath, "ilk-sifre"));
    }

    [Fact]
    public async Task SetPasswordAsync_yazilamayan_yolda_dostane_hata_dondurur()
    {
        var sut = new PasswordService();
        var invalidPath = _tempDir + "\0invalid\\sfr.lock";

        var result = await sut.SetPasswordAsync(invalidPath, "gizli123");

        Assert.False(result.Success);
        Assert.Equal("Yönetici kurulumu yalnızca sunucuda yapılabilir.", result.ErrorMessage);
    }

    [Fact]
    public async Task VerifyPasswordAsync_dosya_yoksa_false_doner_hata_firlatmaz()
    {
        var sut = new PasswordService();

        var result = await sut.VerifyPasswordAsync(_lockPath, "herhangi-bir-sey");

        Assert.False(result);
    }

    [Fact]
    public void IsPasswordSet_dosya_yoksa_false()
    {
        var sut = new PasswordService();

        Assert.False(sut.IsPasswordSet(_lockPath));
    }

    [Fact]
    public async Task IsPasswordSet_kurulumdan_sonra_true()
    {
        var sut = new PasswordService();
        await sut.SetPasswordAsync(_lockPath, "gizli123");

        Assert.True(sut.IsPasswordSet(_lockPath));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
