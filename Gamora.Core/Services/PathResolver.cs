using Gamora.Core.Abstractions;
using Gamora.Core.Models;

namespace Gamora.Core.Services;

public sealed class PathResolver : IPathResolver
{
    public string Resolve(string pathTemplate, LauncherSettings settings)
    {
        return pathTemplate.Replace("{GAMEDISK}", settings.GameDisk, StringComparison.OrdinalIgnoreCase);
    }
}
