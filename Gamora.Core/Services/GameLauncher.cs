using Gamora.Core.Abstractions;
using Gamora.Core.Models;
using Serilog;

namespace Gamora.Core.Services;

public sealed class GameLauncher : IGameLauncher
{
    private readonly IReadOnlyDictionary<LaunchType, ILaunchStrategy> _strategies;
    private readonly ISettingsService _settingsService;

    public GameLauncher(IEnumerable<ILaunchStrategy> strategies, ISettingsService settingsService)
    {
        _strategies = strategies.ToDictionary(s => s.LaunchType);
        _settingsService = settingsService;
    }

    public async Task<LaunchResult> LaunchAsync(Game game, CancellationToken cancellationToken = default)
    {
        if (!_strategies.TryGetValue(game.LaunchType, out var strategy))
        {
            Log.Error("Desteklenmeyen başlatma tipi: {LaunchType} ({GameId})", game.LaunchType, game.Id);
            return LaunchResult.Failure("Oyun başlatılamadı. Personele haber verin.");
        }

        var settings = await _settingsService.LoadAsync(cancellationToken);
        return await strategy.LaunchAsync(game, settings, cancellationToken);
    }
}
