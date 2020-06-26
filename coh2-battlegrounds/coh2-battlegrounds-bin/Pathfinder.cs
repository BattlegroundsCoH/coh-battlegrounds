using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds {
    
    /// <summary>
    /// Pathfinder for finding the paths to relevant paths (eg. CoH2's documents folder)
    /// </summary>
    public static class Pathfinder {

        private static string[] steampaths;

        /// <summary>
        /// 
        /// </summary>
        public static string SteamPath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static string CoHPath { get; private set; }

        static Pathfinder() {

            steampaths = new string[0];
            SteamPath = string.Empty;
            CoHPath = string.Empty;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetOrFindSteamPath() {

            if (SteamPath.CompareTo(string.Empty) != 0) {
                return SteamPath;
            }

            short A = (short)'A';
            short Z = (short)'Z';

            List<string> paths = new List<string>();

            for (short c = A; c < Z; c++) {
                if (_driveHasSteam((char)c, out int t)) {
                    paths.Add(GetSteamPath((char)c, t));
                }
            }

            steampaths = paths.ToArray();

            string steampath = paths.Find(x => File.Exists(x + "Steam.exe"));

            if (steampath != null) {
                SteamPath = steampath + "Steam.exe";
            }

            return SteamPath;

        }

        static bool _driveHasSteam(char c, out int t) {

            bool a = File.Exists($"{c}:\\SteamLibrary\\Steam.dll");
            bool b = File.Exists($"{c}:\\Program Files\\SteamLibrary\\Steam.dll");
            bool d = File.Exists($"{c}:\\Program Files (x86)\\SteamLibrary\\Steam.dll");
            bool e = File.Exists($"{c}:\\Program Files\\Steam\\Steam.dll");
            bool f = File.Exists($"{c}:\\Program Files (x86)\\Steam\\Steam.dll");

            t = -1;

            if (a) t = 0;
            if (b) t = 1;
            if (d) t = 2;
            if (e) t = 3;
            if (f) t = 4;

            return t != -1;

        }

        static string GetSteamPath(char c, int t) {
            string[] ex = {
                $"{c}:\\SteamLibrary\\",
                $"{c}:\\Program Files\\SteamLibrary\\",
                $"{c}:\\Program Files (x86)\\SteamLibrary\\",
                $"{c}:\\Program Files\\Steam\\",
                $"{c}:\\Program Files (x86)\\Steam\\",
            };
            return ex[t];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetOrFindCoHPath() {

            if (CoHPath != string.Empty) {
                return CoHPath;
            }

            if (steampaths.Length == 0) {
                GetOrFindSteamPath();
            }

            for (int i = 0; i < steampaths.Length; i++) {
                if (Directory.Exists(steampaths[i] + "Steamapps\\Common\\Company of Heroes 2\\")) {
                    CoHPath = steampaths[i] + "Steamapps\\Common\\Company of Heroes 2\\";
                    return CoHPath;
                }
            }

            return CoHPath;

        }

    }

}
