using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace Battlegrounds;

/// <summary>
/// Pathfinder for finding the paths to relevant paths (eg. CoH2's documents folder)
/// </summary>
public static class Pathfinder {

    private static string[] steampaths;

    /// <summary>
    /// Get the path of Steam
    /// </summary>
    public static string SteamPath { get; private set; }

    /// <summary>
    /// Get the path of CoH2
    /// </summary>
    public static string CoHPath { get; private set; }

    static Pathfinder() {

        steampaths = Array.Empty<string>();
        SteamPath = string.Empty;
        CoHPath = string.Empty;

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
            Trace.WriteLine($"Detected Steam install path: {CoHPath}", nameof(Pathfinder));
        }

        // Return found path
        return SteamPath;

    }

    // Checks if there's a folder with the Steam DLL
    static bool DriveHasSteam(char c, out int t) {

        // Collect bool flags
        bool a = File.Exists($"{c}:\\Steam\\Steam.dll");
        bool b = File.Exists($"{c}:\\SteamLibrary\\Steam.dll");
        bool d = File.Exists($"{c}:\\Program Files\\SteamLibrary\\Steam.dll");
        bool e = File.Exists($"{c}:\\Program Files (x86)\\SteamLibrary\\Steam.dll");
        bool f = File.Exists($"{c}:\\Program Files\\Steam\\Steam.dll");
        bool g = File.Exists($"{c}:\\Program Files (x86)\\Steam\\Steam.dll");

        // Any flag
        t = -1;

        // Set
        if (a) t = 0;
        if (b) t = 1;
        if (d) t = 2;
        if (e) t = 3;
        if (f) t = 4;
        if (g) t = 5;

        // Return if any
        return t != -1;

    }

    static string GetSteamPath(char c, int t) {
        string[] ex = {
            $"{c}:\\Steam\\",
            $"{c}:\\SteamLibrary\\",
            $"{c}:\\Program Files\\SteamLibrary\\",
            $"{c}:\\Program Files (x86)\\SteamLibrary\\",
            $"{c}:\\Program Files\\Steam\\",
            $"{c}:\\Program Files (x86)\\Steam\\",
        };
        return ex[t];
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

}
