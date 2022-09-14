using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using Battlegrounds.AI.Lobby;

namespace Battlegrounds.AI;

/// <summary>
/// Static utility class representing a database for AI data.
/// </summary>
public static class AIDatabase {

    private static bool __isLoaded = false;
    private static bool __isLoading = false;

    private static Dictionary<string, AIMapAnalysis> __mapanalysis = new();

    /// <summary>
    /// Load the physical file contents into the AI database.
    /// </summary>
    public static void LoadAIDatabase() {

        // If loading, wait
        while (__isLoading) {
            Thread.Sleep(10);
        }

        // If already loaded
        if (__isLoaded) {
            return;
        }

        // Mark as loading
        __isLoading = true;

        // Grab ai filepath and make sure it exists
        var map_profiles_path = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.DATABASE_FOLDER, "vcoh-aimap-db.json");
        if (File.Exists(map_profiles_path)) {
            __mapanalysis = JsonSerializer.Deserialize<Dictionary<string, AIMapAnalysis>>(File.OpenRead(map_profiles_path)) ?? new();
        }

        // Set flags
        __isLoaded = true;
        __isLoading = false;

    }

    /// <summary>
    /// Get the analysis of the specified scenario map.
    /// </summary>
    /// <param name="scenarioName">The relative filename of the scenario to get analysis data from.</param>
    /// <returns>The <see cref="AIMapAnalysis"/> data or <see langword="null"/> if not analysis is found.</returns>
    public static AIMapAnalysis? GetMapAnalysis(string scenarioName) { 
        
        // Load if not loaded
        if (!__isLoaded) {
            LoadAIDatabase();
        }

        // Return analysis or default
        return __mapanalysis.GetValueOrDefault(scenarioName);

    }

}
