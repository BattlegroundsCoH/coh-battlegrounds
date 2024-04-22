using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Games.Scenarios;

public class BaseScenario : IScenario {
    
    public LobbyScenario AsProto() {
        return new LobbyScenario { ScenarioFilename = "", ScenarioName = "", ScenarioPlayercount = 2 };
    }

}
