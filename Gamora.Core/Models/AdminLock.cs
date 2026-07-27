namespace Gamora.Core.Models;

// dataRoot'taki sfr.lock dosyasının içeriği. Salt + iterasyon sayısı + hash birlikte
// tutulur ki ileride iterasyon sayısı artırıldığında eski kayıtlar hâlâ doğrulanabilsin.
public sealed class AdminLock
{
    public string Algorithm { get; set; } = "PBKDF2-SHA256";
    public string Salt { get; set; } = "";
    public int Iterations { get; set; }
    public string Hash { get; set; } = "";
}
