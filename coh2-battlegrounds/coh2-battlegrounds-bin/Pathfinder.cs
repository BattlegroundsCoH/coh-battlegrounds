using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace Battlegrounds;

/// <summary>
/// Pathfinder for finding the paths to relevant paths (eg. Steam install path and CoH2's install path)
/// </summary>
public static class Pathfinder {

    // Registered paths for Steam
    private static string[] steampaths;

    // Paths we try to guess from.
    private static readonly string[] autopaths = {
        "Steam\\",
        "SteamLibrary\\",
        "Program Files\\SteamLibrary\\",
        "Program Files (x86)\\SteamLibrary\\",
        "Program Files\\Steam\\",
        "Program Files (x86)\\Steam\\",
        "Games\\Steam\\",
    };

    /// <summary>
    /// Get or set the path of Steam
    /// </summary>
    public static string SteamPath { get; set; }

    /// <summary>
    /// Get or set the path of CoH2
    /// </summary>
    public static string CoHPath { get; set; }

    static Pathfinder() {

        steampaths = Array.Empty<string>();
        try {
            SteamPath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.STEAM_FOLDER);
            CoHPath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.COH_FOLDER);
        } catch {
            SteamPath = string.IsNullOrEmpty(SteamPath) ? string.Empty : SteamPath;
            CoHPath = string.Empty;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetOrFindSteamPath() { // We could use registry data here to detect the path
                                                // but that would make it slightly harder to detect CoH2 path

        // Check if path is already found
        if (!string.IsNullOrEmpty(SteamPath)) {
            return SteamPath;
        }

        // Define range
        byte A = (byte)'A';
        byte Z = (byte)'Z';

        // Collected paths
        List<string> paths = new List<string>();

        // Run from drives A-Z
        for (byte c = A; c < Z; c++) {
            if (DriveHasSteam((char)c, out int t)) {
                paths.Add(GetSteamPath((char)c, t));
            }
        }

        // Set paths
        steampaths = paths.ToArray();

        // Get complete steam path
        string? steampath = paths.FirstOrDefault(x => File.Exists(Path.Combine(x, "Steam.exe")));

        // Set path
        if (!string.IsNullOrEmpty(steampath)) {
            SteamPath = steampath + "Steam.exe";
            Trace.WriteLine($"Detected Steam install path: {SteamPath}", nameof(Pathfinder));
        }

        // Return found path
        return SteamPath;

    }

    // Checks if there's a folder with the Steam DLL
    static bool DriveHasSteam(char c, out int t) {

        // Set default
        t = -1;

        // Loop through and check
        for (int i = 0; i < autopaths.Length; i++) {
            if (File.Exists($"{c}:\\{autopaths[i]}Steam.dll")) {
                t = i;
                return true;
            }
        }

        // Return if any
        return false;

    }

    static string GetSteamPath(char c, int t) => $"{c}:\\{autopaths[t]}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool VerifySteamPath(string path) {
        if (path.ToLowerInvariant().EndsWith("steam.exe")) {
            return File.Exists(path);
        } else {
            return File.Exists(Path.Combine(path, "Steam.exe"));
        }
    }

    /// <summary>
    /// Finds the locally installed CoH2 path based on found Steam paths
    /// </summary>
    /// <returns></returns>
    public static string GetOrFindCoHPath() {

        // If any found already, return
        if (!string.IsNullOrEmpty(CoHPath)) {
            return CoHPath;
        }

        // If no steam paths, find them first
        if (steampaths.Length == 0) {
            GetOrFindSteamPath();
        }

        // Loop over steam paths and find first instance contaning the CoH2 path
        for (int i = 0; i < steampaths.Length; i++) {
            if (File.Exists(steampaths[i] + "Steamapps\\Common\\Company of Heroes 2\\RelicCoH2.exe")) {
                CoHPath = steampaths[i] + "Steamapps\\Common\\Company of Heroes 2\\";
                Trace.WriteLine($"Detected CoH2 install path: {CoHPath}", nameof(Pathfinder));
                return CoHPath;
            }
        }

        // Return empty
        return CoHPath;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cohpath"></param>
    /// <returns></returns>
    public static bool VerifyCoHPath(string cohpath) {
        bool hasexe = cohpath.ToLowerInvariant().EndsWith("reliccoh2.exe");
        bool basicVerify = hasexe ? File.Exists(cohpath) : File.Exists(Path.Combine(cohpath, "RelicCoH2.exe"));
        if (!basicVerify) {
            return false;
        }
        string steamcheck = hasexe ? (Path.GetDirectoryName(cohpath) ?? "") : cohpath;
        return File.Exists(Path.GetFullPath(Path.Combine(steamcheck, "..\\..\\..\\Steam.dll")));
    }

}
