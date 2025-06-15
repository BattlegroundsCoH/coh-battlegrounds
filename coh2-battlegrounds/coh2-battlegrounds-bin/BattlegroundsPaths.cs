namespace Battlegrounds;

/// <summary>
/// Static class containing constant path ID strings to use in <see cref="BattlegroundsContext.GetRelativePath(string, string)"/>.
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
    /// Path ID for the temporary extract folder
    /// </summary>
    public const string EXTRACT_FOLDER = "extract_folder";

    /// <summary>
    /// Path ID of the art folder containing the shipped GFX data
    /// </summary>
    public const string MOD_ART_FOLDER = "art_folder";

    /// <summary>
    /// Path ID of the install folder
    /// </summary>
    public const string INSTALL_FOLDER = "install_folder";

    /// <summary>
    /// Path ID for the user-generated mods
    /// </summary>
    public const string MOD_USER_FOLDER = "mods_folder";

    /// <summary>
    /// Path ID for the user-generated mod(s) database
    /// </summary>
    public const string MOD_USER_DATABASE_FODLER = "mods_db_folder";

    /// <summary>
    /// Path ID for the user-generated icons folder
    /// </summary>
    public const string MOD_USER_ICONS_FODLER = "mods_ico_folder";

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

    /// <summary>
    /// Path ID for the steam folder
    /// </summary>
    public const string STEAM_FOLDER = "steam_folder";

    /// <summary>
    /// Path ID for the CoH2 folder
    /// </summary>
    public const string COH2_FOLDER = "coh_folder";

    /// <summary>
    /// Path ID for the CoH3 folder
    /// </summary>
    public const string COH3_FOLDER = "coh_3_folder";

    /// <summary>
    /// Path ID for the documents folder
    /// </summary>
    public const string DOCUMENTS_FOLDER = "doc_folder";

    /// <summary>
    /// Get the temp folder
    /// </summary>
    public const string TEMP_FOLDER = "~tmp";

    /// <summary>
    /// Get the update folder
    /// </summary>
    public const string UPDATE_FOLDER = "update";

    /// <summary>
    /// Path ID for the coh resource folder (ie. where we store ripped/synthesised coh resources)
    /// </summary>
    public const string COH_RESOURCES_FODLER = "coh_resources_folder";

}
