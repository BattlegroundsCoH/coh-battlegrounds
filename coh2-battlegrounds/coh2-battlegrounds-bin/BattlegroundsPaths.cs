namespace Battlegrounds;

/// <summary>
/// Static class containing constant path ID strings to use in <see cref="BattlegroundsInstance.GetRelativePath(string, string)"/>.
/// </summary>
public static class BattlegroundsPaths {

    /// <summary>
    /// Path ID for the company folder
    /// </summary>
    public const string COMPANY_FOLDER = "company_folder";

    /// <summary>
    /// Path ID for the tmporary build folder
    /// </summary>
    public const string BUILD_FOLDER = "build_folder";

    /// <summary>
    /// Path ID of the temporary session folder
    /// </summary>
    public const string SESSION_FOLDER = "session_folder";

    /// <summary>
    /// Path ID of the art folder containing the shipped GFX data
    /// </summary>
    public const string MOD_ART_FOLDER = "art_folder";

    /// <summary>
    /// Path ID for the user-generated mods
    /// </summary>
    public const string MOD_OTHER_FOLDER = "mods_folder";

    /// <summary>
    /// Path ID for the plugin folder
    /// </summary>
    public const string PLUGIN_FOLDER = "plugins_folder";

    /// <summary>
    /// Get the database folder (Not the same as the blueprints database!)
    /// </summary>
    public const string DATABASE_FOLDER = "database_folder";

    /// <summary>
    /// Get the binary folder - containing shipped binary files (bg_common)
    /// </summary>
    public const string BINARY_FOLDER = "bin_folder";

    /// <summary>
    /// Path ID for the locale folder
    /// </summary>
    public const string LOCALE_FOLDER = "locale_folder";

}
