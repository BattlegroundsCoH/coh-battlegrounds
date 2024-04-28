using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Scenarios;

namespace Battlegrounds.Core.Services.Standard;

public class ScenarioService : IScenarioService {
    
    public IScenario? GetScenario(IGame game, string scenarioFilename) {
        return new BaseScenario();
    }

    public IScenario[] GetScenarios(IGame game) {
        return [];
    }

}
