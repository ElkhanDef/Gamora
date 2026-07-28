using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class GameIdGeneratorTests
{
    [Fact]
    public void GenerateUniqueId_basit_adi_kucuk_harfli_slug_yapar()
    {
        var id = GameIdGenerator.GenerateUniqueId("Counter-Strike 2", []);

        Assert.Equal("counter-strike-2", id);
    }

    [Fact]
    public void GenerateUniqueId_turkce_karakterleri_sadelestirir()
    {
        var id = GameIdGenerator.GenerateUniqueId("Öykü Şövalyesi Çılgın", []);

        Assert.Equal("oyku-sovalyesi-cilgin", id);
    }

    [Fact]
    public void GenerateUniqueId_carpisan_id_icin_sayi_ekler()
    {
        var id = GameIdGenerator.GenerateUniqueId("Valorant", ["valorant", "valorant-2"]);

        Assert.Equal("valorant-3", id);
    }

    [Fact]
    public void GenerateUniqueId_carpismayan_id_oldugu_gibi_doner()
    {
        var id = GameIdGenerator.GenerateUniqueId("Valorant", ["cs2", "lol"]);

        Assert.Equal("valorant", id);
    }

    [Fact]
    public void GenerateUniqueId_tamamen_ozel_karakterli_ad_icin_yedek_kullanir()
    {
        var id = GameIdGenerator.GenerateUniqueId("!!!", []);

        Assert.Equal("oyun", id);
    }
}
