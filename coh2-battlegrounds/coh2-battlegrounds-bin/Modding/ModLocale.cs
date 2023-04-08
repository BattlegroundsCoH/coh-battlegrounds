using System;
using System.IO;
using System.Text.Json.Serialization;

using Battlegrounds.Game.DataSource;
using Battlegrounds.Logging;

namespace Battlegrounds.Modding;

public readonly struct ModLocale {

    private static readonly Logger logger = Logger.CreateLogger();

    [JsonIgnore]
    public ModType ModType => Enum.Parse<ModType>(this.Type);
    
    public string Type { get; }
    
    public string Path { get; }

    [JsonConstructor]
    public ModLocale(string Type, string Path) {
        this.Type = Type;
        this.Path = Path;
    }

    public UcsFile? GetLocale(string modID, string language) {
        string locFile = this.Path.Replace("%LANG%", language);
        try {
            string locpath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, locFile);
            string locpath2 = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_USER_FOLDER, locFile);
            if (File.Exists(locpath)) {
                return UcsFile.LoadFromFile(locpath);
            } else if (File.Exists(locpath2)) {
                return UcsFile.LoadFromFile(locpath2);
            } else if (locpath != BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_USER_FOLDER)) {
                logger.Warning($"Failed to locate ucs file '{locpath}'", nameof(ModPackage));
            }
        } catch (Exception locex) {
            logger.Warning($"Failed to load ucs file for mod package '{modID}' ({locex.Message})", nameof(ModPackage));
        }
        return null;
    }

}
