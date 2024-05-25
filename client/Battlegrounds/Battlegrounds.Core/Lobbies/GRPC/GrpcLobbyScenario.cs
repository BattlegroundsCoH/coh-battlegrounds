using Battlegrounds.Core.Games.Scenarios;
using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobbyScenario(LobbyScenario scenario) : IScenario {

    public string Name => scenario.ScenarioName;

    public string Description => throw new NotImplementedException();

    public string FileName => scenario.ScenarioFilename;

    public int PlayerCount => scenario.ScenarioPlayercount;

    public LobbyScenario AsProto() => scenario;

    public override string ToString() => Name;

}
