namespace Gamora.Core.Abstractions;

public interface IStatsService
{
    Task RecordLaunchAsync(string statsDirectory, string gameId, CancellationToken cancellationToken = default);
}
