namespace Gamora.Core.Models;

// IsPlatformFallback + FallbackPlatformLabel: launchTarget boş bırakılan steam/riot/battlenet/epic
// oyunlarında strateji belirli bir oyunu değil platformun kendisini açar (bkz. ilgili
// LaunchStrategy'ler). Bu iki alan sadece test modu loglamasında ve LaunchStrategyBase'in kendi
// akışında kullanılır; TestMode'da gerçek komutun yerine notepad.exe geçtiği için kaybolurlar.
public sealed record LaunchCommand(
    string FileName,
    string? Arguments = null,
    string? WorkingDirectory = null,
    bool UseShellExecute = false,
    bool IsPlatformFallback = false,
    string? FallbackPlatformLabel = null)
{
    public string Describe() => string.IsNullOrEmpty(Arguments) ? FileName : $"{FileName} {Arguments}";

    public static LaunchCommand ForFile(string fileName) => new(fileName);
}
