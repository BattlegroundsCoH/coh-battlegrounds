using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public interface ISquadCrew {

    ushort? SquadIndex { get; }

    SquadBlueprint Blueprint { get; }

}
