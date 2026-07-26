using Gamora.Core.Models;

namespace Gamora.Core.Abstractions;

public interface ISettingsService
{
    Task<LauncherSettings> LoadAsync(CancellationToken cancellationToken = default);
}
