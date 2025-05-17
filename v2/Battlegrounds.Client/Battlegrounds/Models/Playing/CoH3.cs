namespace Battlegrounds.Models.Playing;

public sealed class CoH3(Configuration configuration) : Game, ICoH3Game {

    public const string GameId = "coh3";

    public override string GameName => "Company of Heroes 3";

    public override string AppExecutableFullPath => "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3\\RelicCoH3.exe"; /* TODO: Get app path from configuration */

    public string ModProjectPath => "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\bg_wincondition.coh3mod"; /* TODO: Get project path from configuration */

    public string MatchDataPath => "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\assets\\scar\\winconditions\\match_data.scar"; /* TODO: Get match data path from configuration */

    public override string ArchiverExecutable => "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3\\EssenceEditor.exe"; // AKA Essence Editor

}
