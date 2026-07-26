namespace Gamora.Core.Models;

public sealed record LaunchCommand(string FileName, string? Arguments = null, string? WorkingDirectory = null, bool UseShellExecute = false)
{
    public string Describe() => string.IsNullOrEmpty(Arguments) ? FileName : $"{FileName} {Arguments}";

    public static LaunchCommand ForFile(string fileName) => new(fileName);
}
