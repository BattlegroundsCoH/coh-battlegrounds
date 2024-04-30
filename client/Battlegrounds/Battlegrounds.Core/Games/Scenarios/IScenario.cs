using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Games.Scenarios;

public interface IScenario {

    string Name { get; }

    string Description { get; }

    string FileName { get; }

    int PlayerCount { get; }

    LobbyScenario AsProto();

}
