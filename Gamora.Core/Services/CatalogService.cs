using System.Text.Json;
using System.Text.Json.Serialization;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class CatalogService : ICatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task<Catalog> LoadAsync(string catalogPath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(catalogPath);
        var catalog = await JsonSerializer.DeserializeAsync<Catalog>(stream, JsonOptions, cancellationToken);
        return catalog ?? throw new InvalidDataException($"catalog.json okunamadı veya boş: {catalogPath}");
    }

    public async Task SaveAsync(Catalog catalog, string catalogPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(catalogPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = catalogPath + ".tmp";

        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, catalog, JsonOptions, cancellationToken);
        }

        if (File.Exists(catalogPath))
        {
            File.Replace(tempPath, catalogPath, destinationBackupFileName: null);
        }
        else
        {
            File.Move(tempPath, catalogPath);
        }
    }
}
