namespace Battlegrounds.Models.Lobbies;

public enum TeamType : byte {
    Allies = 0,
    Axis = 1,
}

public sealed record Team(TeamType TeamType, string TeamAlias, Team.Slot[] Slots) {
    
    public sealed record Slot(int Index, string? ParticipantId, string Faction, string CompanyId, string Difficulty, bool Hidden, bool Locked);

    public HashSet<string> Participants => [.. Slots.Select(x => x.ParticipantId)];

}
