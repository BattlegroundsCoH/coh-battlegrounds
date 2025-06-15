namespace Battlegrounds.Models.Companies;

public sealed class Company {

    private readonly List<Squad> _squads = [];

    public string Id { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime UpdatedAt { get; init; } = DateTime.Now;

    public string Name { get; init; } = string.Empty;

    public string Faction { get; init; } = string.Empty;

    public string GameId { get; init; } = string.Empty;

    public IReadOnlyList<Squad> Squads {
        get => _squads.AsReadOnly();
        init => _squads = [.. value];
    }

}
