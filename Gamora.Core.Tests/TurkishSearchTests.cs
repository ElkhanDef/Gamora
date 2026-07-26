using Gamora.Core.Services;

namespace Gamora.Core.Tests;

public class TurkishSearchTests
{
    [Theory]
    [InlineData("valorant", "VALORANT")]
    [InlineData("İstanbul", "istanbul")]
    [InlineData("FIFA", "fifa")]
    [InlineData("Çılgın Köşk", "CILGIN KOSK")]
    [InlineData("Şüphe", "suphe")]
    public void Normalize_farkli_yazimlari_ayni_sonuca_indirger(string a, string b)
    {
        Assert.Equal(TurkishSearch.Normalize(a), TurkishSearch.Normalize(b));
    }

    [Fact]
    public void Normalize_bos_metinde_hata_vermez()
    {
        Assert.Equal("", TurkishSearch.Normalize(""));
    }
}
