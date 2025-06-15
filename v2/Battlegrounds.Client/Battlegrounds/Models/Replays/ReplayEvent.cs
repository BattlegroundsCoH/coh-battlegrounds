namespace Battlegrounds.Models.Replays;

public abstract record ReplayEvent(TimeSpan Timestamp, ReplayPlayer? Player);
public sealed record UnknownReplayEvent(TimeSpan Timestamp, string EventType, Dictionary<string, object> Details) : ReplayEvent(Timestamp, null);

public sealed record SquadDeployedEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);
public sealed record SquadKilledEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);
public sealed record SquadWeaponPickupEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId, string WeaponName, bool IsEntityBlueprint) : ReplayEvent(Timestamp, Player);
public sealed record SquadRecalledEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId, float Experience, int InfantryKills, int VehicleKills) : ReplayEvent(Timestamp, Player);
public sealed record MatchPlayerOverEvent(TimeSpan Timestamp, ReplayPlayer Player, bool IsWinner, List<SquadRecalledEvent> DeployedUnits) : ReplayEvent(Timestamp, Player);
public sealed record MatchStartReplayEvent(TimeSpan Timestamp, string MatchId, string ModVersion, string Scenario, List<MatchStartReplayEvent.PlayerData> Players) : ReplayEvent(Timestamp, null) {
    public sealed record PlayerData(int PlayerId, string Name, string CompanyId, int ModId);
}
public sealed record MatchOverReplayEvent(TimeSpan Timestamp, List<int> Winners, List<int> Losers, List<MatchOverReplayEvent.PlayerStatistics> PlayerStats) : ReplayEvent(Timestamp, null) {
    public sealed record PlayerStatistics(int PlayerId, int TeamId, string Name, int ModId, int Kills);
}
