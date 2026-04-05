namespace Battlegrounds.Models.Replays;

public abstract record ReplayEvent(TimeSpan Timestamp, ReplayPlayer? Player);
public sealed record UnknownReplayEvent(TimeSpan Timestamp, string EventType, Dictionary<string, object> Details) : ReplayEvent(Timestamp, null);

public sealed record SquadDeployedEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);
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
public sealed record SquadRecalledEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId, float Experience, int InfantryKills, int VehicleKills, int EntityLosses) : ReplayEvent(Timestamp, Player);
public sealed record MatchStartReplayEvent(TimeSpan Timestamp, string MatchId, string ModVersion, string Scenario, List<MatchStartReplayEvent.PlayerData> Players) : ReplayEvent(Timestamp, null) {
    public sealed record PlayerData(int PlayerId, string Name, string CompanyId, int ModId);
}
public sealed record MatchOverReplayEvent(TimeSpan Timestamp, List<int> Winners, List<int> Losers, List<MatchOverReplayEvent.PlayerStatistics> PlayerStats) : ReplayEvent(Timestamp, null) {
    public sealed record PlayerStatistics(int PlayerId, int TeamId, string Name, int ModId, int Kills, int Losses);
}
