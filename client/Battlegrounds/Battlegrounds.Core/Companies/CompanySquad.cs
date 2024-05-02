using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public sealed class CompanySquad(
    ushort squadId, 
    SquadBlueprint blueprint, 
    string? name, 
    float experience, 
    IReadOnlySet<PropertyBagGroupId> upgrades, 
    IReadOnlyList<PropertyBagGroupId> items,
    ISquadTransport? transport = null,
    ISquadCrew? crew = null) : ISquad {

    public ushort SquadId => squadId;

    public SquadBlueprint Blueprint => blueprint;

    public string? Name => name;

    public float Experience => experience;

    public IReadOnlySet<PropertyBagGroupId> Upgrades => upgrades;

    public IReadOnlyList<PropertyBagGroupId> Items => items;

    public ISquadCrew? Crew => crew;

    public ISquadTransport? Transport => transport;

}
