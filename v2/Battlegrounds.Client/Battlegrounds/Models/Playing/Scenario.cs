namespace Battlegrounds.Models.Playing;

/// <summary>
/// Represents a scenario configuration, including localized name, description, player limits, and preview information.
/// </summary>
/// <remarks>Use this type to define the metadata and constraints for a scenario, such as its display name,
/// description, and the maximum number of players allowed. All properties are immutable and must be set during
/// initialization.</remarks>
public sealed class Scenario {

    /// <summary>
    /// Gets the localized name associated with this instance.
    /// </summary>
    public LocaleString Name { get; init; }
    
    /// <summary>
    /// Gets the localized description for the current object.
    /// </summary>
    public LocaleString Description {  get; init; }
    
    /// <summary>
    /// Gets the maximum number of players allowed in the game session.
    /// </summary>
    public int MaxPlayers { get; init; }
    
    /// <summary>
    /// Gets a read-only preview of the content associated with this instance.
    /// </summary>
    public string Preview { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the scenario associated with this instance.
    /// </summary>
    public string ScenarioName { get; init; } = string.Empty;

}
