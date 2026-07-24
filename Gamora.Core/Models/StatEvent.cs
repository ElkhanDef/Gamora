namespace Gamora.Core.Models;

// stats/{MachineName}.jsonl dosyasına bir satır olarak eklenir (JSON Lines).
public sealed class StatEvent
{
    public string GameId { get; set; } = "";
    public string MachineName { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
}
