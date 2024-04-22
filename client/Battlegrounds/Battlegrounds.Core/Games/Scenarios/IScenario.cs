using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Games.Scenarios;

public interface IScenario {

    LobbyScenario AsProto();

}
