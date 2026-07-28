using Gamora.Core.Models;

namespace Gamora.Core.Abstractions;

public interface IPathResolver
{
    string Resolve(string pathTemplate, LauncherSettings settings);

    string ToTemplate(string actualPath, LauncherSettings settings);
}
