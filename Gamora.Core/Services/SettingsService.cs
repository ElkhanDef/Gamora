using System.Text.Json;
using System.Text.Json.Serialization;
using Gamora.Core.Abstractions;
using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _settingsPath;
    private LauncherSettings? _cached;

    public SettingsService() : this(Path.Combine(AppContext.BaseDirectory, "settings.json"))
    {
    }

    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
    }

    public async Task<LauncherSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        if (!File.Exists(_settingsPath))
        {
            _cached = new LauncherSettings();
            await using var createStream = File.Create(_settingsPath);
            await JsonSerializer.SerializeAsync(createStream, _cached, JsonOptions, cancellationToken);
            return _cached;
        }

        await using var stream = File.OpenRead(_settingsPath);
        _cached = await JsonSerializer.DeserializeAsync<LauncherSettings>(stream, JsonOptions, cancellationToken) ?? new LauncherSettings();
        return _cached;
    }
}
