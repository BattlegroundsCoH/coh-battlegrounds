using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Logging;
using Battlegrounds.Modding;
using Battlegrounds.Steam;
using Battlegrounds.Update;

namespace Battlegrounds;

/// <summary>
/// Class representation of the Battlegrounds .dll instance
/// </summary>
public static class BattlegroundsInstance {

    // The path of the local settings file
    private static readonly string PATH_LOCAL = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH2\\local.json");

    // The logger instance
    private static readonly Logger logger = Logger.CreateLogger();

    // ID of other settings
    public const string OPT_ZOOM = "ingame_zoom";
    public const string OPT_AUTOSCAR = "auto_scar";
    public const string OPT_AUTOUPDATE = "auto_update";
    public const string OPT_AUTODATA = "auto_data";
    public const string OPT_AUTOWORKSHOP = "auto_workshop";

    /// <summary>
    /// Internal instance object
    /// </summary>
    internal class InternalInstance {

        public Dictionary<string, string> Paths { get; set; }

        public string LastPlayedScenario { get; set; }

        public string LastPlayedGamemode { get; set; }

        public Dictionary<string, string> LastPlayedCompany { get; set; }

        public int LastPlayedGamemodeSetting { get; set; }

        public Dictionary<string, object> OtherOptions { get; set; }

        public SteamInstance SteamData { get; set; }

        public LocaleLanguage Language { get; set; }

        /// <summary>
        /// Initialize a new <see cref="InternalInstance"/> class with default data.
        /// </summary>
        public InternalInstance() {
            this.SteamData = new();
            this.Paths = new();
            this.LastPlayedCompany = new() {
                [Faction.Soviet.Name] = string.Empty,
                [Faction.Wehrmacht.Name] = string.Empty,
                [Faction.OberkommandoWest.Name] = string.Empty,
                [Faction.America.Name] = string.Empty,
                [Faction.British.Name] = string.Empty,
            };
            this.LastPlayedGamemode = "bg_vp";
            this.LastPlayedGamemodeSetting = 1;
            this.LastPlayedScenario = string.Empty;
            this.OtherOptions = new() {
                [OPT_AUTODATA] = false,
                [OPT_AUTOSCAR] = false,
                [OPT_AUTOUPDATE] = false,
                [OPT_AUTOWORKSHOP] = false, // Don't do this automatically --> it makes the error log look annoying
                [OPT_ZOOM] = 0.0
            };
        }

        /// <summary>
        /// Resolve paths for internal use.
        /// </summary>
        public void ResolvePathsAndInitLog() {

            // Paths
            string doc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH2\\");
            string installpath = $"{Environment.CurrentDirectory}\\";
            string binpath = $"{installpath}bg_common\\";
            string userpath = $"{doc}usr\\";
            string tmppath = $"{doc}~tmp\\";

            // Create documents directory if it does not exist
            if (!Directory.Exists(doc)) {
                Directory.CreateDirectory(doc);
                this.Paths.Add(BattlegroundsPaths.DOCUMENTS_FOLDER, doc);
            } else {
                if (!this.Paths.ContainsKey(BattlegroundsPaths.DOCUMENTS_FOLDER)) {
                    this.Paths.Add(BattlegroundsPaths.DOCUMENTS_FOLDER, doc);
                }
            }

            if (!this.Paths.ContainsKey(BattlegroundsPaths.TEMP_FOLDER)) {
                this.Paths.Add(BattlegroundsPaths.TEMP_FOLDER, tmppath);
            }

            // Create logger
            __logger = new();

            // Log
            logger.Info($"Resolving paths (Local to: {Environment.CurrentDirectory})");

            // Create data directory if it does not exist
            if (!Directory.Exists(binpath)) {
                Directory.CreateDirectory(binpath);
                logger.Info("Bin path missing - this may cause errors");
                this.Paths.Add(BattlegroundsPaths.BINARY_FOLDER, binpath);
            } else {
                if (!this.Paths.ContainsKey(BattlegroundsPaths.BINARY_FOLDER)) {
                    this.Paths.Add(BattlegroundsPaths.BINARY_FOLDER, binpath);
                }
            }

            // Create user directory if it does not exist
            if (!Directory.Exists(userpath)) {
                Directory.CreateDirectory(userpath);
                logger.Info("User path missing - this may cause errors");
            }

            // Install folder
            this.ResolveDirectory(BattlegroundsPaths.INSTALL_FOLDER, installpath);

            // User folder
            this.ResolveDirectory(BattlegroundsPaths.COMPANY_FOLDER, $"{doc}companies\\");
            this.ResolveDirectory(BattlegroundsPaths.MOD_USER_FOLDER, $"{userpath}mods\\");
            this.ResolveDirectory(BattlegroundsPaths.MOD_USER_DATABASE_FODLER, $"{userpath}mods\\mod_db\\");
            this.ResolveDirectory(BattlegroundsPaths.MOD_USER_ICONS_FODLER, $"{userpath}mods\\map_icons\\");

            // Plugin folder
            this.ResolveDirectory(BattlegroundsPaths.PLUGIN_FOLDER, "plugins\\");

            // Data folder
            this.ResolveDirectory(BattlegroundsPaths.MOD_ART_FOLDER, $"{binpath}gfx\\");
            this.ResolveDirectory(BattlegroundsPaths.DATABASE_FOLDER, $"{binpath}data\\");
            this.ResolveDirectory(BattlegroundsPaths.LOCALE_FOLDER, $"{binpath}locale\\");

            // Create tmp directory if it does not exist
            if (!Directory.Exists(tmppath)) {
                Directory.CreateDirectory(tmppath);
            } else { // does exist --> clear it
                // Clear temp folder
                try {
                    Directory.GetFiles(tmppath).ForEach(File.Delete);
                    Directory.GetDirectories(tmppath).ForEach(x => Directory.Delete(x, true));
                } catch {
                    logger.Info("Unexpected IO error occured while attempting to clean tmp folder!");
                }
            }

            // Create tmp folder
            this.ResolveDirectory(BattlegroundsPaths.BUILD_FOLDER, $"{tmppath}bld\\");
            this.ResolveDirectory(BattlegroundsPaths.SESSION_FOLDER, $"{tmppath}ses\\");
            this.ResolveDirectory(BattlegroundsPaths.EXTRACT_FOLDER, $"{tmppath}-extract\\");
            this.ResolveDirectory(BattlegroundsPaths.UPDATE_FOLDER, $"{tmppath}update\\");

        }

        private void ResolveDirectory(string pathID, string defaultPath) {
            try {
                bool found = this.Paths.TryGetValue(pathID, out string? folder);
                // If not found, found was not properly defined, or no longer exist, we create it
                if (!found || string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) {
                    this.Paths[pathID] = defaultPath;
                    Directory.CreateDirectory(this.Paths[pathID]);
                }
            } catch (Exception e) {
                logger.Info($"Failed to resolve directory \"{pathID}\"");
                logger.Exception(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathID"></param>
        /// <returns></returns>
        public string GetPath(string pathID) {
            if (this.Paths.TryGetValue(pathID, out string? path)) {
                return path;
            } else {
                throw new ArgumentException($"Invalid path ID \"{pathID}\"");
            }
        }

        public string ToJsonReference() => throw new NotSupportedException();

    }

    private static LoggerOutput? __logger;
    private static InternalInstance __instance;
    private static Localize __localeManagement;
    private static Random __rng;

    /// <summary>
    /// Get or set the last played map.
    /// </summary>
    public static string LastPlayedMap {
        get => __instance.LastPlayedScenario;
        set => __instance.LastPlayedScenario = value;
    }

    /// <summary>
    /// Get or set the last played gamemode.
    /// </summary>
    public static string LastPlayedGamemode {
        get => __instance.LastPlayedGamemode;
        set => __instance.LastPlayedGamemode = value;
    }

    /// <summary>
    /// Get or set the last played gamemode setting
    /// </summary>
    public static int LastPlayedGamemodeSetting {
        get => __instance.LastPlayedGamemodeSetting;
        set => __instance.LastPlayedGamemodeSetting = value;
    }

    /// <summary>
    /// Get or set the value of other options
    /// </summary>
    public static Dictionary<string, object> OtherOptions {
        get => __instance.OtherOptions;
        set => __instance.OtherOptions = value;
    }

    /// <summary>
    /// Get the random number generator instance.
    /// </summary>
    public static Random RNG => __rng;

    /// <summary>
    /// Get if this is the first time the application has been launched
    /// </summary>
    public static bool IsFirstRun { get; internal set; }

    /// <summary>
    /// Get the active <see cref="SteamInstance"/>
    /// </summary>
    public static SteamInstance Steam => __instance.SteamData;

    /// <summary>
    /// Get the localize manager for the instance
    /// </summary>
    public static Localize Localize => __localeManagement;

    /// <summary>
    /// Get the activer logger instance
    /// </summary>
    public static LoggerOutput? Log => __logger;

    /// <summary>
    /// Get and set the assembly version of executable.
    /// </summary>
    public static IAppVersionFetcher? Version { get; set; }

#if DEBUG

    /// <summary>
    /// The battlegrounds debug instance for easy debug info. Not available in release mode and should always be
    /// enclosed by a debug directive
    /// </summary>
    public static readonly BattlegroundsDebug Debug = new();

#endif

    /// <summary>
    /// Set a specific path for the instance
    /// </summary>
    /// <param name="pathId">The ID of the path to save</param>
    /// <param name="path">The actual path to set</param>
    public static void SaveInstancePath(string pathId, string path) {
        if (pathId is not BattlegroundsPaths.STEAM_FOLDER and not BattlegroundsPaths.COH_FOLDER) {
            return;
        }
        __instance.Paths[pathId] = path;
    }

    /// <summary>
    /// Get the relative path to a predefined app path. Can be appened with remaining direct path to obtain the full path.
    /// </summary>
    /// <param name="pathId">The <see cref="string"/> path ID.</param>
    /// <param name="appendPath">The optional path to append to the relative path.</param>
    /// <returns>The relative path + potential append path.</returns>
    /// <exception cref="ArgumentException"/>
    public static string GetRelativePath(string pathId, string appendPath = "")
        => Path.Combine(__instance.GetPath(pathId), appendPath);

    /// <summary>
    /// Get the relative path based on virtual path.
    /// </summary>
    /// <param name="path">The path to construct absolute path from</param>
    /// <param name="extension">Optional extension to append.</param>
    /// <returns>The absolute path to the virtual path.</returns>
    public static string GetRelativeVirtualPath(string path, string extension = "") {

        // Grab relative
        int rel = path.IndexOf(':');
        string relative = (rel is not -1 ? path[..rel] : string.Empty) switch {
            "Gamemode" => GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "bg_wc"),
            "Common" => GetRelativePath(BattlegroundsPaths.BINARY_FOLDER),
            _ => GetRelativePath(BattlegroundsPaths.INSTALL_FOLDER)
        };

        // Cut last
        if (relative[^1] is '\\') {
            relative = relative[..^1];
        }

        // Create path
        StringBuilder pathBuilder = new(relative);

        // Split
        var dotted = (rel is not -1 ? path[(rel+1)..] : path).Split('.');

        // Create path from dots
        for (int i = 0; i < dotted.Length; i++) {
            pathBuilder.Append('\\');
            pathBuilder.Append(dotted[i]);
        }

        // If an extension is desired, we add it
        if (!string.IsNullOrEmpty(extension)) {
            pathBuilder.Append(extension);
        }

        // Return path
        return pathBuilder.ToString();

    }

    /// <summary>
    /// Static constructor
    /// </summary>
    static BattlegroundsInstance() {
        try {
            LoadInstance();
        } catch {
            __instance = new InternalInstance();
            __localeManagement = new Localize(__instance.Language);
            __rng = new();
        }
    }

    /// <summary>
    /// Load the current instance data.
    /// </summary>
    [MemberNotNull(nameof(__instance), nameof(__localeManagement), nameof(__rng))]
    public static void LoadInstance() {

        // Make sure we do not run this again
        if (__instance is not null && __localeManagement is not null && __rng is not null) {
            return;
        }

        // Grab documents path
        string doc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH2");

        // Check if directory exist
        bool hasDoc = Directory.Exists(doc);
        if (hasDoc && File.Exists(PATH_LOCAL)) {

            // Load instance data
            var instance = JsonSerializer.Deserialize<InternalInstance?>(File.ReadAllText(PATH_LOCAL));
            if (instance is null) {
                __instance = new InternalInstance();
                __instance.ResolvePathsAndInitLog();
                IsFirstRun = true;
            } else {
                IsFirstRun = false;
                __instance = instance;
                __instance.ResolvePathsAndInitLog();
            }

            // Fix settings
            foreach (var (k,v) in __instance.OtherOptions) {
                if (v is JsonElement je) {
                    __instance.OtherOptions[k] = je.ValueKind switch {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.String => je.GetString() ?? string.Empty,
                        JsonValueKind.Number => je.GetDouble(),
                        _ => throw new Exception("Invalid JSON entry in options")
                    };
                }
            }

        } else {

            // Yep, new instance
            __instance = new InternalInstance();
            __instance.ResolvePathsAndInitLog();
            IsFirstRun = true;

        }

        // Create locale manager
        __localeManagement = new Localize(__instance.Language);

        // Load mods
        ModManager.Init();

        // Create RNG
        __rng = new Random();

    }

    /// <summary>
    /// Verify if the given <see cref="SteamUser"/> is the local user.
    /// </summary>
    /// <param name="user">The <see cref="SteamUser"/> to verify.</param>
    /// <returns>Will return <see langword="true"/> if local user. Otherwise <see langword="false"/>.</returns>
    public static bool IsLocalUser(SteamUser user) => __instance.SteamData.User.ID == user.ID;

    /// <summary>
    /// Verify if the given user ID is the local user ID.
    /// </summary>
    /// <param name="userID">The user ID to verify.</param>
    /// <returns>Will return <see langword="true"/> if local user. Otherwise <see langword="false"/>.</returns>
    public static bool IsLocalUser(ulong userID) => __instance.SteamData.User.ID == userID;

    /// <summary>
    /// Save the currently stored data of this instance.
    /// </summary>
    public static void SaveInstance()
        => File.WriteAllText(PATH_LOCAL, JsonSerializer.Serialize(__instance, new JsonSerializerOptions() { WriteIndented = true }));

    public static void ChangeLanguage(LocaleLanguage lang) => __instance.Language = lang;

}
