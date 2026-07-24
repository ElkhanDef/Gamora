using Gamora.Core.Models;

namespace Gamora.Core.Abstractions;

public interface ICatalogService
{
    Task<Catalog> LoadAsync(string catalogPath, CancellationToken cancellationToken = default);

    Task SaveAsync(Catalog catalog, string catalogPath, CancellationToken cancellationToken = default);
}
