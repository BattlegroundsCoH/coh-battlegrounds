namespace Battlegrounds.Core.Games.Factions;

public sealed class CoH2Faction(byte factionIndex, string name, string alliance, string scarReferenceId) : IFaction {

    public byte FactionIndex => factionIndex;

    public string Name => name;

    public string Alliance => alliance;

    public string GameId => throw new NotImplementedException();

    public string ScarReferenceId => scarReferenceId;

    public override string ToString() => name;

}
