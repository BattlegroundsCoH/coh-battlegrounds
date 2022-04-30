using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Steam;

namespace Battlegrounds;

/// <summary>
/// Class representation of the Battlegrounds .dll instance
/// </summary>
public static class BattlegroundsInstance {

    /// <summary>
    /// Internal instance object
    /// </summary>
    internal class InternalInstance {

        public Dictionary<string, string> Paths { get; set; }

        public string LastPlayedScenario { get; set; }

        public string LastPlayedGamemode { get; set; }

        public Dictionary<string, string> LastPlayedCompany { get; set; }

        public int LastPlayedGamemodeSetting { get; set; }

        public Dictionary<string, string> OtherOptions { get; set; }

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
            this.OtherOptions = new();
        }

        /// <summary>
        /// Resolve paths for internal use.
        /// </summary>
        public void ResolvePaths() {

            // Log
            Trace.WriteLine($"Resolving paths (Local to: {Environment.CurrentDirectory})", nameof(BattlegroundsInstance));

            // Paths
            string installpath = $"{Environment.CurrentDirectory}\\";
            string binpath = $"{installpath}bg_common\\";
            string userpath = $"{installpath}usr\\";
            string tmppath = $"{installpath}~tmp\\";

            // Create data directory if it does not exist
            if (!Directory.Exists(binpath)) {
                Directory.CreateDirectory(binpath);
                Trace.WriteLine("Bin path missing - this may cause errors", nameof(BattlegroundsInstance));
                this.Paths.Add(BattlegroundsPaths.BINARY_FOLDER, binpath);
            } else {
                if (!this.Paths.ContainsKey(BattlegroundsPaths.BINARY_FOLDER)) {
                    this.Paths.Add(BattlegroundsPaths.BINARY_FOLDER, binpath);
                }
            }

            // Create user directory if it does not exist
            if (!Directory.Exists(userpath)) {
                Directory.CreateDirectory(userpath);
                Trace.WriteLine("User path missing - this may cause errors", nameof(BattlegroundsInstance));
            }

            // User folder
            this.ResolveDirectory(BattlegroundsPaths.COMPANY_FOLDER, $"{userpath}companies\\");
            this.ResolveDirectory(BattlegroundsPaths.MOD_OTHER_FOLDER, $"{userpath}mods\\");
            this.ResolveDirectory(BattlegroundsPaths.PLUGIN_FOLDER, $"{userpath}plugins\\");

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
                    Trace.WriteLine("Unexpected IO error occured while attempting to clean tmp folder!", nameof(BattlegroundsInstance));
                }
            }

            // Create tmp folder
            this.ResolveDirectory(BattlegroundsPaths.BUILD_FOLDER, $"{tmppath}bld\\");
            this.ResolveDirectory(BattlegroundsPaths.SESSION_FOLDER, $"{tmppath}ses\\");

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
                Trace.WriteLine($"Failed to resolve directory \"{pathID}\"", nameof(BattlegroundsInstance));
                Trace.WriteLine(e, nameof(BattlegroundsInstance));
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
    /// Get or set other last played options
    /// </summary>
    public static Dictionary<string, string> OtherOptions {
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
    /// <param name="pathID">The <see cref="string"/> path ID.</param>
    /// <param name="appendPath">The optional path to append to the relative path.</param>
    /// <returns>The relative path + potential append path.</returns>
    /// <exception cref="ArgumentException"/>
    public static string GetRelativePath(string pathId, string appendPath = "")
        => Path.Combine(__instance.GetPath(pathId), appendPath);

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

        // Load instance data
        bool hasLocal = File.Exists("local.json");
        var instance = hasLocal.Then(() => JsonSerializer.Deserialize<InternalInstance?>(File.ReadAllText("local.json"))).Else(_ => null);
        if (instance is null) {
            __instance = new InternalInstance();
            __instance.ResolvePaths();
            IsFirstRun = true;
        } else {
            IsFirstRun = false;
            __instance = instance;
            __instance.ResolvePaths();
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
        => File.WriteAllText("local.json", JsonSerializer.Serialize(__instance, new JsonSerializerOptions() { WriteIndented = true }));

}
