using System.Text.Json.Serialization;

using Battlegrounds.Models.Companies;

namespace Battlegrounds.Models.Replays;

/// <summary>
/// Represents an event indicating a mismatch or invalid condition during a replay operation.
/// </summary>
/// <remarks>This event is typically used to signal that a replay operation encountered an issue, such as an 
/// unexpected state or invalid data. The <see cref="Reason"/> property provides additional context  about the nature of
/// the mismatch.</remarks>
/// <param name="Event">The replay event associated with the mismatch.</param>
/// <param name="Reason">A description of the reason for the mismatch. This value cannot be null or empty.</param>
public sealed record BadMatchEvent(ReplayEvent Event, string Reason);

/// <summary>
/// Represents the result of a match, including its metadata, participants, and associated events.
/// </summary>
/// <remarks>This class provides detailed information about a match, such as its duration, scenario, and
/// associated events. It also includes mappings between players and their companies, as well as collections of winners
/// and losers. Use this class to analyze match outcomes, validate match data, or retrieve metadata for reporting
/// purposes.</remarks>
public sealed class MatchResult {

    public static readonly MatchResult Unknown = new MatchResult();
    public static readonly MatchResult Invalid = new MatchResult() { IsValid = false };

    /// <summary>
    /// Gets a value indicating whether the current state is valid.
    /// </summary>
    public bool IsValid { get; init; } = true;

    /// <summary>
    /// Gets or sets the unique identifier for the lobby where the match took place.
    /// </summary>
    public string LobbyId { get; set; } = string.Empty; // Unique identifier for the lobby where the match took place

    /// <summary>
    /// Gets the unique identifier for the game.
    /// </summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier for the match.
    /// </summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version of the mod as a string.
    /// </summary>
    public string ModVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the scenario played in the match.
    /// </summary>
    public string Scenario { get; init; } = string.Empty; // Name of the scenario played in the match

    /// <summary>
    /// Gets the total duration of the match.
    /// </summary>
    public TimeSpan MatchDuration { get; init; } = TimeSpan.Zero; // Total duration of the match

    /// <summary>
    /// Gets the list of events that indicate issues with the match.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<BadMatchEvent> BadEvents { get; init; } = []; // List of events that indicate issues with the match

    /// <summary>
    /// Gets the collection of company event modifiers associated with each player, keyed by player ID.
    /// </summary>
    public IReadOnlyDictionary<string, LinkedList<CompanyEventModifier>> CompanyModifiers { get; init; } = new Dictionary<string, LinkedList<CompanyEventModifier>>(); // Events associated with each player, keyed by player ID

    /// <summary>
    /// Gets a read-only dictionary that maps player IDs to their associated company IDs.
    /// </summary>
    public IReadOnlyDictionary<string, string> PlayerCompanies { get; init; } = new Dictionary<string, string>(); // Mapping of player IDs to their company IDs

    /// <summary>
    /// Gets the set of player IDs who won the match.
    /// </summary>
    public IReadOnlySet<string> Winners { get; init; } = new HashSet<string>(); // Set of player IDs who won the match

    /// <summary>
    /// Gets the set of player IDs who lost the match.
    /// </summary>
    public IReadOnlySet<string> Losers { get; init; } = new HashSet<string>(); // Set of player IDs who lost the match

    /// <summary>
    /// Gets a value indicating whether the match has concluded.
    /// </summary>
    public bool Concluded { get; init; } = false; // Indicates if the match has concluded (e.g., victory or defeat)

}
