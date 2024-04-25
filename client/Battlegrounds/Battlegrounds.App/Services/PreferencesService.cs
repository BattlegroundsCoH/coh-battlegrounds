using Battlegrounds.Core.Configuration;
using Battlegrounds.Core.Games;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.App.Services;

public sealed class PreferencesService(ILogger<PreferencesService> logger, Config config) {

    public string GetLastSkirmishScenarioName(IGame game) =>
        Preferences.Get($"last_played_scenario_{game.Name}", game.DefaultScenario);

    public IDictionary<string, string> GetLastSkirmishSettings(IGame game) => game.SkirmishSettings.ToDictionary(x => x.Item1, x => Preferences.Get($"last_played_{x.Item1}_{game.Name}", x.Item2));

}
