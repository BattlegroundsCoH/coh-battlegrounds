using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Battlegrounds.Functional;
using Battlegrounds.Json;
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

            /// <summary>
            /// 
            /// </summary>
            [JsonReference(typeof(SteamUser))] public SteamUser User { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public Dictionary<string, string> Paths { get; set; }

            public string LastPlayedScenario { get; set; }

            public string LastPlayedGamemode { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public InternalInstance() {
                this.User = null;
                this.Paths = new Dictionary<string, string>();
            }

            /// <summary>
            /// 
            /// </summary>
            [JsonOnDeserialized]
            public void ResolvePaths() {

                Trace.WriteLine($"Resolving paths (Local to: {Environment.CurrentDirectory})");

                string installpath = $"{Environment.CurrentDirectory}\\";
                string binpath = $"{installpath}bin\\";
                string userpath = $"{installpath}usr\\";
                string tmppath = $"{installpath}~tmp\\";

                // Create data directory if it does not exist
                if (!Directory.Exists(binpath)) {
                    Directory.CreateDirectory(binpath);
                    Trace.WriteLine("Bin path missing - this may cause errors");
                }

                // Create user directory if it does not exist
                if (!Directory.Exists(userpath)) {
                    Directory.CreateDirectory(userpath);
                    Trace.WriteLine("User path missing - this may cause errors");
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
                        Trace.WriteLine("Unexpected IO error occured while attempting to clean tmp folder!");
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
                    Trace.WriteLine($"Failed to resolve directory \"{pathID}\"");
                    Trace.WriteLine(e);
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

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string ToJsonReference() => throw new NotSupportedException();

        }

        private static InternalInstance __instance;

        /// <summary>
        /// The current local steam user instance.
        /// </summary>
        public static SteamUser LocalSteamuser {
            get => __instance.User;
            set => __instance.User = value;
        }

        /// <summary>
        /// The last played map.
        /// </summary>
        public static string LastPlayedMap {
            get => __instance.LastPlayedScenario;
            set => __instance.LastPlayedScenario = value;
        }

        /// <summary>
        /// The last played gamemode.
        /// </summary>
        public static string LastPlayedGamemode {
            get => __instance.LastPlayedGamemode;
            set => __instance.LastPlayedGamemode = value;
        }

        /// <summary>
        /// Get if this is the first time the application has been launched
        /// </summary>
        public static bool IsFirstRun { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathID"></param>
        /// <param name="appendPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string pathID, string appendPath)
            => Path.Combine(__instance.GetPath(pathID), appendPath);

        private static ITuningMod __bgTuningInstance;

        public static ITuningMod BattleGroundsTuningMod => __bgTuningInstance;

        /// <summary>
        /// 
        /// </summary>
        public static void LoadInstance() {

            __instance = JsonParser.ParseFile<InternalInstance>("local.json");
            if (__instance == null) {
                __instance = new InternalInstance();
                __instance.ResolvePaths();
                IsFirstRun = true;
            } else {
                IsFirstRun = false;
            }

            __bgTuningInstance = new BattlegroundsTuning();

        }

        /// <summary>
        /// 
        /// </summary>
        public static void SaveInstance() 
            => File.WriteAllText("local.json", (__instance as IJsonObject).Serialize());

    }

}
