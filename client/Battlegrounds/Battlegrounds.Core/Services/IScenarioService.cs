using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Scenarios;

namespace Battlegrounds.Core.Services;

public interface IScenarioService {

    IScenario? GetScenario(IGame game, string scenarioFilename);

    IList<IScenario> GetScenarios(IGame game);

    Task<bool> LoadFromStreamAsync(Stream? stream);

}
