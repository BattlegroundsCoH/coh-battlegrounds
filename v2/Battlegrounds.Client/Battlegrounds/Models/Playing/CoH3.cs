using System.IO;

namespace Battlegrounds.Models.Playing;

public sealed class CoH3(Configuration configuration) : Game, ICoH3Game {

    public static readonly string[] Factions = ["british_africa", "afrika_korps", "german", "american"];

    public const string GameId = "coh3";

    public override string Id => GameId;

    public override string GameName => "Company of Heroes 3";

    public override string AppExecutableFullPath => Path.Combine(configuration.CoH3.InstallPath, "RelicCoH3.exe");

    public string ModProjectPath => configuration.CoH3.ModProjectPath;

    public string MatchDataPath => configuration.CoH3.MatchDataPath;

    public override string ArchiverExecutable => Path.Combine(configuration.CoH3.InstallPath, "EssenceEditor.exe"); // AKA Essence Editor

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

    public override string GetFactionName(string factionId) => factionId switch {
        "british_africa" => "British",
        "afrika_korps" => "Afrika Korps",
        "german" => "German",
        "american" => "American",
        _ => throw new ArgumentException($"Unknown faction ID: {factionId}")
    };

}
