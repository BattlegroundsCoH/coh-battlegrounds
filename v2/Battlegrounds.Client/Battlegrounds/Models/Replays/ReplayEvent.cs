namespace Battlegrounds.Models.Replays;

public abstract record ReplayEvent(TimeSpan Timestamp, ReplayPlayer? Player);

public sealed record SquadDeployedEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);
public sealed record SquadKilledEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId) : ReplayEvent(Timestamp, Player);
public sealed record SquadWeaponPickupEvent(TimeSpan Timestamp, ReplayPlayer Player, ushort SquadCompanyId, string WeaponName, bool IsEntityBlueprint) : ReplayEvent(Timestamp, Player);
public sealed record MatchStartReplayEvent(TimeSpan Timestamp, string Scenario, List<MatchStartReplayEvent.PlayerData> Players) : ReplayEvent(Timestamp, null) {
    public sealed record PlayerData(int PlayerId, string Name, string CompanyId, int ModId);
}
