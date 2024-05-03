namespace Battlegrounds.Core.Games.Factions;

public sealed class CoH3Faction(byte factionIndex, string name, string alliance) : IFaction {

    public static readonly CoH3Faction British = new CoH3Faction(0, "British", "allies");
    public static readonly CoH3Faction American = new CoH3Faction(1, "American", "allies");
    public static readonly CoH3Faction Wehrmacht = new CoH3Faction(2, "Wehrmacht", "axis");
    public static readonly CoH3Faction AfrikaKorps = new CoH3Faction(3, "Afrika Korps", "axis");

    public byte FactionIndex => factionIndex;

    public string Name => name;

    public string Alliance => alliance;

}
