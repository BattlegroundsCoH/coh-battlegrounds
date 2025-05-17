namespace Battlegrounds.Models.Companies;

public sealed class Company {

    private readonly List<Squad> _squads = [];

    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Squad> Squads {
        get => _squads.AsReadOnly();
        init => _squads = [.. value];
    }

}
