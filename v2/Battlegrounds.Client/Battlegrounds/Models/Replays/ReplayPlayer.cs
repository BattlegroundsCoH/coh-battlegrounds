namespace Battlegrounds.Models.Replays;

public sealed record ReplayPlayer(int PlayerId, int TeamId, int SlotId, string PlayerName, ulong ProfileId, ulong SteamId, string Faction, string AIProfile, bool IsHuman = false);
