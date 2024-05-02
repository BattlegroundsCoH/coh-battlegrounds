using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public interface ISquadTransport {

    SquadBlueprint Blueprint { get; }

    ushort? UnitTransportIndex { get; }

}
