using Battlegrounds.Models.Playing;

namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents a playable map configuration, including its name, description, player limit, preview image, and
/// associated scenario.
/// </summary>
/// <param name="Name">The display name of the map.</param>
/// <param name="Description">A brief description of the map's features or setting.</param>
/// <param name="MaxPlayers">The maximum number of players allowed on the map. Must be a positive integer.</param>
/// <param name="Preview">A URI or path to the preview image representing the map.</param>
/// <param name="ScenarioName">The name of the scenario associated with the map.</param>
public sealed record Map(string Name, string Description, int MaxPlayers, string Preview, string ScenarioName) {
    
    /// <summary>
    /// Creates a new Map instance based on the properties of the specified scenario.
    /// </summary>
    /// <param name="scenario">The scenario from which to initialize the map. Must not be null.</param>
    /// <returns>A Map object initialized with the name, description, maximum players, preview, and scenario name from the
    /// specified scenario.</returns>
    public static Map FromScenario(Scenario scenario) => new Map(scenario.Name, scenario.Description, scenario.MaxPlayers, scenario.Preview, scenario.ScenarioName);

}
