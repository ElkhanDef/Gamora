namespace Gamora.Core.Models;

// stats/{MachineName}.jsonl dosyasına bir satır olarak eklenir (JSON Lines).
// Makine kimliği kaydın içinde yok — zaten dosya adında, tekrar yazılmıyor.
public sealed class StatEvent
{
    public string GameId { get; set; } = "";
    public string Event { get; set; } = "launch";
    public string Time { get; set; } = "";
}
