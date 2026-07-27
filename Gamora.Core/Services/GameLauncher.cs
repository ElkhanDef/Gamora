using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.Core.Services;

public sealed class GameLauncher : IGameLauncher
{
    private readonly IReadOnlyDictionary<LaunchType, ILaunchStrategy> _strategies;
    private readonly ISettingsService _settingsService;
    private readonly IStatsService _statsService;

    public GameLauncher(IEnumerable<ILaunchStrategy> strategies, ISettingsService settingsService, IStatsService statsService)
    {
        _strategies = strategies.ToDictionary(s => s.LaunchType);
        _settingsService = settingsService;
        _statsService = statsService;
    }

    public async Task<LaunchResult> LaunchAsync(Game game, CancellationToken cancellationToken = default)
    {
        if (!_strategies.TryGetValue(game.LaunchType, out var strategy))
        {
            Log.Error("Desteklenmeyen başlatma tipi: {LaunchType} ({GameId})", game.LaunchType, game.Id);
            return LaunchResult.Failure("Oyun başlatılamadı. Personele haber verin.");
        }

        var settings = await _settingsService.LoadAsync(cancellationToken);
        var result = await strategy.LaunchAsync(game, settings, cancellationToken);

        if (result.Success)
        {
            // Fire-and-forget: istatistik yazımı başlatma akışını bir milisaniye bile
            // bekletmez. StatsService kendi içinde tüm hataları yakalar, buraya asla fırlamaz.
            _ = _statsService.RecordLaunchAsync(settings.StatsPath, game.Id, CancellationToken.None);
        }

        return result;
    }
}
