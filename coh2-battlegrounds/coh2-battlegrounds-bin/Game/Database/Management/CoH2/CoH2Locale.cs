using System.IO;

using Battlegrounds.Game.Database.Management.Common;
using Battlegrounds.Game.DataSource;

namespace Battlegrounds.Game.Database.Management.CoH2;

/// <summary>
/// 
/// </summary>
public class CoH2Locale : CommonLocale {

    /// <summary>
    /// 
    /// </summary>
    public static readonly UcsFile Locale = GetCoH2Locale();

    /// <inheritdoc/>
    public override string GetString(uint localeKey) 
        => Locale[localeKey];

    private static UcsFile GetCoH2Locale() {
        
        // Determine language to use
        string language = BattlegroundsContext.Localize.Language.ToString();
        if (language == "Default") {
            language = "English";
        }

        // Load VCoh Locale
        string localePath = Path.Combine(Pathfinder.GetOrFindCoHPath(), $"CoH2\\Locale\\{language}\\RelicCoH2.{language}.ucs");
        if (File.Exists(localePath)) {
            return UcsFile.LoadFromFile(localePath);
        } else {
            throw new FileNotFoundException($"Failed to load locale file '{localePath}'");
        }

    }

}
