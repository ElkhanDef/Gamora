namespace Gamora.Core.Models;

public sealed class Game
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Cover { get; set; }
    public LaunchType LaunchType { get; set; }
    public string LaunchTarget { get; set; } = "";
    public string? WorkingDir { get; set; }
    public string? Args { get; set; }
    public bool Visible { get; set; } = true;
    public int SortOrder { get; set; }
    public bool AgeRestricted { get; set; }
}
