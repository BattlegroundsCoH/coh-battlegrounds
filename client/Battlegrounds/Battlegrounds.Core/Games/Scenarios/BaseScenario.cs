using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Games.Scenarios;

public class BaseScenario : IScenario {

    public required string Name { get; init; } = "$0";

    public required string Description { get; init; } = "$0";

    public required string FileName { get; init; }

    public required int PlayerCount { get; init; } = 2;

    public LobbyScenario AsProto() {
        return new LobbyScenario { ScenarioFilename = FileName, ScenarioName = Name, ScenarioPlayercount = PlayerCount };
    }

    public override string ToString() => FileName;

}
