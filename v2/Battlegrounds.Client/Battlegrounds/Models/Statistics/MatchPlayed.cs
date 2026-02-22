namespace Battlegrounds.Models.Statistics;

/// <summary>
/// Represents a record of a completed match, including details such as the game, player, outcome, and statistics.
/// </summary>
/// <remarks>This type is used to capture information about a match after it has been played, including metadata
/// and player performance. All properties are required and immutable, ensuring the integrity of match data. Typical
/// usage involves storing or analyzing match history for a player or game.</remarks>
public sealed class MatchPlayed {

    /// <summary>
    /// Gets the unique identifier for the match.
    /// </summary>
    public required string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the game associated with this instance.
    /// </summary>
    public required string GameId { get; init; } = string.Empty; // CoH3, CoH2, etc.

    /// <summary>
    /// Gets the date and time when the game was played.
    /// </summary>
    public required DateTime DatePlayed { get; init; }

    /// <summary>
    /// Gets the duration of the operation or event represented by this instance.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the unique identifier of the player's company.
    /// </summary>
    public required string PlayerCompanyId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version number assigned to the company record.
    /// </summary>
    public required uint CompanyVersion { get; init; }

    /// <summary>
    /// Gets the faction name associated with the player.
    /// </summary>
    public required string PlayerFaction { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the map that was played.
    /// </summary>
    public required string PlayedMap { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the outcome represents a victory.
    /// </summary>
    public required bool IsVictory { get; init; }

    /// <summary>
    /// Gets a value indicating whether the game is running in single-player mode.
    /// </summary>
    public required bool IsSinglePlayer { get; init; }

    /// <summary>
    /// Gets the total number of losses recorded.
    /// </summary>
    public required int TotalLosses { get; init; }

    /// <summary>
    /// Gets the total number of kills recorded for the entity.
    /// </summary>
    public required int TotalKills { get; init; }

    /// <summary>
    /// Gets the version string of the client application.
    /// </summary>
    public required string ClientVersion { get; init; } = string.Empty;

}
