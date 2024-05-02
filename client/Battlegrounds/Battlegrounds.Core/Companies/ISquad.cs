using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public interface ISquad {

    ushort SquadId { get; }

    string? Name { get; }

    SquadBlueprint Blueprint { get; }

    float Experience { get; }

    ISquadCrew? Crew { get; }

    ISquadTransport? Transport { get; }

    IReadOnlySet<PropertyBagGroupId> Upgrades { get; }

    IReadOnlyList<PropertyBagGroupId> Items { get; }

}
