namespace Gamora.Core.Models;

public sealed class Catalog
{
    public int Version { get; set; } = 1;
    public DateTime UpdatedAt { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<Game> Games { get; set; } = [];
}
