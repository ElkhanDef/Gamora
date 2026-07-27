using System.Text.Json;
using System.Text.Json.Serialization;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.Core.Services;

public sealed class PopularityService : IPopularityService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private Dictionary<string, int> _launchCounts = new();

    public async Task LoadAsync(string statsDirectory, CancellationToken cancellationToken = default)
    {
        var counts = new Dictionary<string, int>();

        if (Directory.Exists(statsDirectory))
        {
            var files = Directory.EnumerateFiles(statsDirectory, "*.jsonl").ToList();
            foreach (var file in files)
            {
                await AccumulateFileAsync(file, counts, cancellationToken);
            }

            Log.Information("Popülerlik hesaplandı: {GameCount} oyun, {FileCount} dosya", counts.Count, files.Count);
        }

        _launchCounts = counts;
    }

    public int GetLaunchCount(string gameId) => _launchCounts.GetValueOrDefault(gameId);

    private static async Task AccumulateFileAsync(string file, Dictionary<string, int> counts, CancellationToken cancellationToken)
    {
        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(file, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "İstatistik dosyası okunamadı: {File}", file);
            return;
        }

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            StatEvent? statEvent;
            try
            {
                statEvent = JsonSerializer.Deserialize<StatEvent>(line, JsonOptions);
            }
            catch (JsonException)
            {
                // Başka bir makine tam o anda yazıyor olabilir; yarım/bozuk satırı sessizce atla.
                continue;
            }

            if (string.IsNullOrEmpty(statEvent?.GameId))
            {
                continue;
            }

            counts[statEvent.GameId] = counts.GetValueOrDefault(statEvent.GameId) + 1;
        }
    }
}
