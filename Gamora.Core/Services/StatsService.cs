using System.Text.Json;
using System.Text.Json.Serialization;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.Core.Services;

public sealed class StatsService : IStatsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _machineName;

    public StatsService() : this(Environment.MachineName)
    {
    }

    public StatsService(string machineName)
    {
        _machineName = machineName;
    }

    // İstatistik kritik değil, oyun başlatma kritik: bu metod ASLA fırlatmaz. Hata olursa
    // sadece loglanır. Çağıran taraf (GameLauncher) bu çağrıyı beklemeden (fire-and-forget)
    // yapmalı ki başlatma akışı bir milisaniye bile gecikmesin.
    public async Task RecordLaunchAsync(string statsDirectory, string gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(statsDirectory);
            var filePath = Path.Combine(statsDirectory, $"{_machineName}.jsonl");

            var statEvent = new StatEvent
            {
                GameId = gameId,
                Event = "launch",
                Time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
            };

            var line = JsonSerializer.Serialize(statEvent, JsonOptions) + Environment.NewLine;
            await File.AppendAllTextAsync(filePath, line, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "İstatistik yazılamadı: {GameId}", gameId);
        }
    }
}
