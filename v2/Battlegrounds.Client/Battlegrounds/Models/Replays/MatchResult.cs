using System.Text.Json.Serialization;

namespace Battlegrounds.Models.Replays;

public sealed record BadMatchEvent(ReplayEvent Event, string Reason);

public sealed class MatchResult {

    public static readonly MatchResult Unknown = new MatchResult();
    public static readonly MatchResult Invalid = new MatchResult() { IsValid = false };

    public bool IsValid { get; init; } = true;

    public string LobbyId { get; set; } = string.Empty; // Unique identifier for the lobby where the match took place

    public string GameId { get; init; } = string.Empty;

    public string MatchId { get; init; } = string.Empty;

    public string ModVersion { get; init; } = string.Empty;

    public string Scenario { get; init; } = string.Empty; // Name of the scenario played in the match

    public TimeSpan MatchDuration { get; init; } = TimeSpan.Zero; // Total duration of the match

    [JsonIgnore]
    public IReadOnlyList<BadMatchEvent> BadEvents { get; init; } = []; // List of events that indicate issues with the match

    public IReadOnlyDictionary<string, LinkedList<ReplayEvent>> PlayerEvents { get; init; } = new Dictionary<string, LinkedList<ReplayEvent>>(); // Events associated with each player, keyed by player ID

    public IReadOnlySet<string> Winners { get; init; } = new HashSet<string>(); // Set of player IDs who won the match

    public IReadOnlySet<string> Losers { get; init; } = new HashSet<string>(); // Set of player IDs who lost the match

    public bool Concluded { get; init; } = false; // Indicates if the match has concluded (e.g., victory or defeat)

}
