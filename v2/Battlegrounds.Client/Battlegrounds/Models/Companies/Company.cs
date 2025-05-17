namespace Battlegrounds.Models.Companies;

public sealed class Company {

    private readonly List<Squad> _squads = [];

    public string Id { get; } = string.Empty;

    public string Name { get; } = string.Empty;

    public IReadOnlyList<Squad> Squads => _squads.AsReadOnly();

}
