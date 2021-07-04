using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Battlegrounds.Functional;
using Battlegrounds.Json;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Steam;

namespace Battlegrounds {

    /// <summary>
    /// Class representation of the Battlegrounds .dll instance
    /// </summary>
    public static class BattlegroundsInstance {

        /// <summary>
        /// Internal instance object
        /// </summary>
        public class InternalInstance : IJsonObject {

            public Dictionary<string, string> Paths { get; set; }

            public string LastPlayedScenario { get; set; }

            public string LastPlayedGamemode { get; set; }

            public int LastPlayedGamemodeSetting { get; set; }

            public SteamInstance SteamData { get; set; }

            [JsonEnum(typeof(LocaleLanguage))]
            public LocaleLanguage Language { get; set; }

            /// <summary>
            /// Initialize a new <see cref="InternalInstance"/> class with default data.
            /// </summary>
            public InternalInstance() {
                this.SteamData = new SteamInstance();
                this.Paths = new Dictionary<string, string>();
                this.LastPlayedGamemode = "Victory Points";
                this.LastPlayedGamemodeSetting = 1;
            }

            /// <summary>
            /// Resolve paths. Automatically called when the json variant is deserialized.
            /// </summary>
            [JsonOnDeserialized]
            public void ResolvePaths() {

                // Log
                Trace.WriteLine($"Resolving paths (Local to: {Environment.CurrentDirectory})", "BattlegroundsInstance");

                // Paths
                string installpath = $"{Environment.CurrentDirectory}\\";
                string binpath = $"{installpath}bin\\";
                string userpath = $"{installpath}usr\\";
                string tmppath = $"{installpath}~tmp\\";

                // Create data directory if it does not exist
                if (!Directory.Exists(binpath)) {
                    Directory.CreateDirectory(binpath);
                    Trace.WriteLine("Bin path missing - this may cause errors", "BattlegroundsInstance");
                    this.Paths.Add(BattlegroundsPaths.BINARY_FOLDER, binpath);
                } else {
                    if (!this.Paths.ContainsKey(BattlegroundsPaths.BINARY_FOLDER)) {
                        this.Paths.Add(BattlegroundsPaths.BINARY_FOLDER, binpath);
                    }
                }

                // Create user directory if it does not exist
                if (!Directory.Exists(userpath)) {
                    Directory.CreateDirectory(userpath);
                    Trace.WriteLine("User path missing - this may cause errors", "BattlegroundsInstance");
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
                        Trace.WriteLine("Unexpected IO error occured while attempting to clean tmp folder!", "BattlegroundsInstance");
                    }
                }

                // Create tmp folder
                this.ResolveDirectory(BattlegroundsPaths.BUILD_FOLDER, $"{tmppath}bld\\");
                this.ResolveDirectory(BattlegroundsPaths.SESSION_FOLDER, $"{tmppath}ses\\");

            }

            private void ResolveDirectory(string pathID, string defaultPath) {
                try {
                    if (!this.Paths.TryGetValue(pathID, out string cFolder) || !Directory.Exists(cFolder)) {
                        if (string.IsNullOrEmpty(cFolder)) {
                            cFolder = defaultPath;
                            this.Paths.Add(pathID, cFolder);
                        } else {
                            this.Paths[pathID] = cFolder;
                        }
                        Directory.CreateDirectory(cFolder);
                    }
                } catch (Exception e) {
                    Trace.WriteLine($"Failed to resolve directory \"{pathID}\"", "BattlegroundsInstance");
                    Trace.WriteLine(e, "BattlegroundsInstance");
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pathID"></param>
            /// <returns></returns>
            public string GetPath(string pathID) {
                if (this.Paths.TryGetValue(pathID, out string path)) {
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
        /// Get the relative path to a predefined app path. Can be appened with remaining direct path to obtain the full path.
        /// </summary>
        /// <param name="pathID">The <see cref="string"/> path ID.</param>
        /// <param name="appendPath">The optional path to append to the relative path.</param>
        /// <returns>The relative path + potential append path.</returns>
        /// <exception cref="ArgumentException"/>
        public static string GetRelativePath(string pathID, string appendPath)
            => Path.Combine(__instance.GetPath(pathID), appendPath);

        private static ITuningMod __bgTuningInstance;

        /// <summary>
        /// Get the Battlegrounds tuning mod instance.
        /// </summary>
        public static ITuningMod BattleGroundsTuningMod => __bgTuningInstance;

        /// <summary>
        /// Load the current instance data.
        /// </summary>
        public static void LoadInstance() {

            // Load instance data
            __instance = JsonParser.ParseFile<InternalInstance>("local.json");
            if (__instance == null) {
                __instance = new InternalInstance();
                __instance.ResolvePaths();
                IsFirstRun = true;
            } else {
                IsFirstRun = false;
            }

            // Create locale manager
            __localeManagement = new Localize(__instance.Language);

            // Create tuning
            __bgTuningInstance = new BattlegroundsTuning();

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
            => File.WriteAllText("local.json", __instance.SerializeAsJson());

    }

}
