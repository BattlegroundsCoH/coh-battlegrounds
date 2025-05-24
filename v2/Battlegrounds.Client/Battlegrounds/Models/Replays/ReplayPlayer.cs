namespace Battlegrounds.Models.Replays;

public sealed record ReplayPlayer(int PlayerId, int TeamId, string PlayerName, ulong ProfileId, ulong SteamId, string Faction, string AIProfile);
