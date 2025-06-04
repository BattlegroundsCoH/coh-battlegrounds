using Battlegrounds.Models.Playing;

namespace Battlegrounds.Models.Lobbies;

public enum TeamType : byte {
    Allies = 0,
    Axis = 1,
}

public sealed record Team(TeamType TeamType, string TeamAlias, Team.Slot[] Slots) {
    
    public sealed record Slot(int Index, string? ParticipantId, string Faction, string CompanyId, AIDifficulty Difficulty, bool Hidden, bool Locked);

    public HashSet<string> Participants => [.. Slots.Where(x => !string.IsNullOrEmpty(x.ParticipantId)).Select(x => x.ParticipantId)];

}
