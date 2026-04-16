namespace Battlegrounds.Models.Replays;

/// <summary>
/// Represents a base event that occurs during a replay, including its timestamp and the associated player, if any.
/// </summary>
/// <param name="Timestamp">The time offset from the start of the replay at which the event occurs.</param>
/// <param name="Player">The player associated with the event, or null if the event is not specific to a player.</param>
public abstract record ReplayEvent(TimeSpan Timestamp, ReplayPlayer? Player);

/// <summary>
/// Represents a replay event with an unrecognized or unsupported event type.
/// </summary>
/// <remarks>Use this type to handle replay events that do not match any known or explicitly supported event
/// types. This allows for forward compatibility and inspection of unexpected event data.</remarks>
/// <param name="Timestamp">The time offset from the start of the replay at which the event occurred.</param>
/// <param name="EventType">The string identifier of the unknown event type.</param>
/// <param name="Details">A dictionary containing additional data associated with the event. The contents and structure depend on the event
/// source.</param>
public sealed record UnknownReplayEvent(TimeSpan Timestamp, string EventType, Dictionary<string, object> Details) : ReplayEvent(Timestamp, null);

/// <summary>
/// Represents an event that occurs when a squad is deployed by a player during a replay.
/// </summary>
/// <param name="Timestamp">The time at which the squad deployment event occurred within the replay.</param>
/// <param name="Player">The player who deployed the squad.</param>
/// <param name="SquadCompanyId">The identifier of the squad company that was deployed.</param>
public sealed record SquadDeployedEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);

/// <summary>
/// Represents an event that occurs when a squad is eliminated during a replay session.
/// </summary>
/// <param name="Timestamp">The time at which the squad was killed, relative to the start of the replay.</param>
/// <param name="Player">The player associated with the squad that was killed. Cannot be null.</param>
/// <param name="SquadCompanyId">The unique identifier of the squad company that was eliminated.</param>
public sealed record SquadKilledEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);

/// <summary>
/// Represents an event where a player picks up a weapon for a squad during a replay session.
/// </summary>
/// <remarks>
/// In Company of Heroes 2, weapon pickups use dedicated Slot Item blueprints.
/// In Company of Heroes 3, weapon pickups are represented as entity blueprints, and the weapon name is stored in the entity's blueprint name.
/// The <see cref="IsEntityBlueprint"/> property indicates whether the weapon is represented as an entity blueprint or a slot item blueprint, allowing for proper handling of the weapon pickup event based on the game version.
/// </remarks>
/// <param name="Timestamp">The time at which the weapon pickup event occurred within the replay.</param>
/// <param name="Player">The player who performed the weapon pickup action.</param>
/// <param name="SquadCompanyId">The identifier of the squad company associated with the weapon pickup.</param>
/// <param name="WeaponName">The name of the weapon that was picked up.</param>
/// <param name="IsEntityBlueprint">true if the weapon is represented as an entity blueprint; otherwise, false.</param>
public sealed record SquadWeaponPickupEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId, string WeaponName, bool IsEntityBlueprint) : ReplayEvent(Timestamp, Player);

/// <summary>
/// Represents an event where a squad captures a team weapon during a replay session.
/// </summary>
/// <param name="TimeSpan">The elapsed time at which the capture event occurs within the replay.</param>
/// <param name="Player">The player who performed the team weapon capture.</param>
/// <param name="EntityName">The name of the team weapon entity that was captured.</param>
/// <param name="SquadName">The name of the squad that captured the team weapon.</param>
/// <param name="Experience">The amount of experience awarded for the capture event.</param>
/// <param name="InfantryKills">The number of infantry units killed during the capture event.</param>
/// <param name="VehicleKills">The number of vehicles destroyed during the capture event.</param>
/// <param name="EntityLosses">The number of entities lost by the capturing squad during the event.</param>
public sealed record SquadTeamWeaponCaptureEvent(TimeSpan TimeSpan, ReplayPlayer Player, string EntityName, string SquadName, float Experience, int InfantryKills, int VehicleKills, int EntityLosses) : ReplayEvent(TimeSpan, Player);

/// <summary>
/// Represents an event indicating that a squad has been recalled during a replay, including details about the player,
/// squad, and related statistics at the time of recall.
/// </summary>
/// <param name="Timestamp">The time at which the squad recall event occurred within the replay.</param>
/// <param name="Player">The player associated with the squad recall event.</param>
/// <param name="SquadCompanyId">The identifier of the squad company that was recalled.</param>
/// <param name="Experience">The amount of experience the squad had accumulated at the time of recall.</param>
/// <param name="InfantryKills">The number of infantry units the squad had killed before being recalled.</param>
/// <param name="VehicleKills">The number of vehicle units the squad had destroyed before being recalled.</param>
/// <param name="EntityLosses">The number of entities lost by the squad prior to recall.</param>
public sealed record SquadRecalledEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId, float Experience, int InfantryKills, int VehicleKills, int EntityLosses) : ReplayEvent(Timestamp, Player);

/// <summary>
/// Represents a replay event that marks the start of a match, including match metadata and player information.
/// </summary>
/// <param name="Timestamp">The time offset from the beginning of the replay at which the match starts.</param>
/// <param name="MatchId">The unique identifier for the match being started.</param>
/// <param name="ModVersion">The version of the mod used for the match. Cannot be null.</param>
/// <param name="Scenario">The scenario or map identifier for the match. Cannot be null.</param>
/// <param name="Players">A list of player data representing all participants in the match. Cannot be null and must contain at least one
/// player.</param>
public sealed record MatchStartReplayEvent(TimeSpan Timestamp, string MatchId, string ModVersion, string Scenario, List<MatchStartReplayEvent.PlayerData> Players) : ReplayEvent(Timestamp, null) {
    
    /// <summary>
    /// Represents immutable player information, including identifiers and associated company and mod data.
    /// </summary>
    /// <param name="PlayerId">The unique identifier for the player.</param>
    /// <param name="Name">The display name of the player. Cannot be null.</param>
    /// <param name="CompanyId">The identifier of the company associated with the player. Cannot be null.</param>
    /// <param name="ModId">The identifier of the mod associated with the player.</param>
    public sealed record PlayerData(int PlayerId, string Name, string CompanyId, int ModId);

}

/// <summary>
/// Represents a replay event that marks the end of a match, including information about winners, losers, and player
/// statistics.
/// </summary>
/// <param name="Timestamp">The time offset from the start of the replay at which the match ended.</param>
/// <param name="Winners">A list of player IDs representing the winners of the match. The list may be empty if there are no winners.</param>
/// <param name="Losers">A list of player IDs representing the losers of the match. The list may be empty if there are no losers.</param>
/// <param name="PlayerStats">A list of player statistics for each participant at the end of the match. Each entry contains details such as player
/// ID, team ID, name, mod ID, kills, and losses.</param>
public sealed record MatchOverReplayEvent(TimeSpan Timestamp, List<int> Winners, List<int> Losers, List<MatchOverReplayEvent.PlayerStatistics> PlayerStats) : ReplayEvent(Timestamp, null) {
    
    /// <summary>
    /// Represents statistical information for a player in a team-based game session.
    /// </summary>
    /// <param name="PlayerId">The unique identifier of the player whose statistics are being recorded.</param>
    /// <param name="TeamId">The identifier of the team to which the player belongs.</param>
    /// <param name="Name">The display name of the player.</param>
    /// <param name="ModId">The identifier of the modification or mode associated with the player's statistics.</param>
    /// <param name="Kills">The total number of kills achieved by the player.</param>
    /// <param name="Losses">The total number of losses or deaths recorded for the player.</param>
    public sealed record PlayerStatistics(int PlayerId, int TeamId, string Name, int ModId, int Kills, int Losses);

}
