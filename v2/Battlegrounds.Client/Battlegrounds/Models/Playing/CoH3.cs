namespace Battlegrounds.Models.Playing;

public sealed class CoH3(Configuration configuration) : Game, ICoH3Game {

    public static readonly string[] Factions = ["british_africa", "afrika_korps", "german", "american"];

    public const string GameId = "coh3";

    public override string Id => GameId;

    public override string GameName => "Company of Heroes 3";

    public override string AppExecutableFullPath => "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3\\RelicCoH3.exe"; /* TODO: Get app path from configuration */

    public string ModProjectPath => "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\bg_wincondition.coh3mod"; /* TODO: Get project path from configuration */

    public string MatchDataPath => "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\assets\\scar\\winconditions\\match_data.scar"; /* TODO: Get match data path from configuration */

    public override string ArchiverExecutable => "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3\\EssenceEditor.exe"; // AKA Essence Editor

    public override string[] FactionIds => Factions;

    public override FactionAlliance GetFactionAlliance(string factionId) {
        if (factionId is "british_africa" or "american") {
            return FactionAlliance.Allies;
        } else if (factionId is "afrika_korps" or "german") {
            return FactionAlliance.Axis;
        } else {
            //throw new ArgumentException($"Unknown faction ID: {factionId}");
            return FactionAlliance.Unspecified; // Default to unspecified for unknown factions
        }
    }

}
