using System.IO;

using Battlegrounds.Game.Database.Management.Common;
using Battlegrounds.Game.DataSource;

namespace Battlegrounds.Game.Database.Management.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3Locale : CommonLocale {

    /// <summary>
    /// 
    /// </summary>
    public static readonly UcsFile Locale = GetCoH3Locale();

    /// <inheritdoc/>
    public override string GetString(uint localeKey) => Locale[localeKey];

    private static UcsFile GetCoH3Locale() {

        // Determine language to use
        string language = BattlegroundsContext.Localize.Language.ToString();
        if (language == "Default") {
            language = "English";
        }

        // Load VCoh Locale
        string localePath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.COH_RESOURCES_FODLER, $"3\\anvil.{language}.ucs");
        if (File.Exists(localePath)) {
            return UcsFile.LoadFromFile(localePath);
        } else {
            throw new FileNotFoundException($"Failed to load locale file '{localePath}'");
        }

    }

}
