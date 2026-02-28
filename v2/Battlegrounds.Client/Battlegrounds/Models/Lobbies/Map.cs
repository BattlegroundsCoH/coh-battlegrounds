using Battlegrounds.Models.Playing;

namespace Battlegrounds.Models.Lobbies;

public sealed record Map(LocaleString Name, LocaleString Description, int MaxPlayers, string Preview, string ScenarioName) {
    
    public static Map FromScenario(Scenario scenario) => new Map(scenario.Name, scenario.Description, scenario.MaxPlayers, scenario.Preview, scenario.ScenarioName);

}
