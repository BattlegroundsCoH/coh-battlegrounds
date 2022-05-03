using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Functional;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Lua.Debugging;

namespace Battlegrounds.Game.Database;

/// <summary>
/// A list of all available scenarios that can be played.
/// </summary>
public static class ScenarioList {

    private static Dictionary<string, Scenario>? __scenarios;

    /// <summary>
    /// Create and load a basic list of scenarios.
    /// </summary>
    public static void LoadList() {

        // Create the dictionary
        __scenarios = new();

        try {

            string rawJsonDb = $"{DatabaseManager.SolveDatabasepath()}vcoh-map-db.json";
            LoadScenarioDatabaseFile(rawJsonDb).ForEach(x => {
                if (!__scenarios.TryAdd(Path.GetFileNameWithoutExtension(x.RelativeFilename), x)) {
                    Trace.WriteLine($"Failed to add duplicate vcoh scenario '{x.RelativeFilename}'", "ScenarioList::LoadList");
                }
            });

            _ = Task.Run(HandleWorkshopFiles);

        } catch (Exception e) {
            Trace.WriteLine(e);
        }

    }

    private static void HandleWorkshopFiles() {

        // Bail if user has not asked to initiate this process
        if (BattlegroundsInstance.OtherOptions.GetCastValueOrDefault(BattlegroundsInstance.OPT_AUTOWORKSHOP, false) is false) {
            
            // Bail
            return;

        }

        // Find path to workshop database
        string workshop_dbpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_USER_DATABASE_FODLER, "workshop-map-db.json");

        // Load existing workshop items
        var workshopScenarios = LoadScenarioDatabaseFile(workshop_dbpath);

        // Load new scenarios
        LoadNewWorkshopScenarios(workshopScenarios, out int newWorkshopEntries);

        // If more than 0, update the database
        if (newWorkshopEntries > 0) {

            // Save database
            File.WriteAllText(workshop_dbpath, JsonSerializer.Serialize(workshopScenarios));

            // Log how many new workship maps were added
            Trace.WriteLine($"Added {newWorkshopEntries} workshop maps.", nameof(ScenarioList));

        }

        // Add all
        workshopScenarios.ForEach(x => __scenarios.IfTrue(y => !(y?.ContainsKey(Path.GetFileNameWithoutExtension(x.RelativeFilename)) ?? false))
            .Then(y => y?.Add(Path.GetFileNameWithoutExtension(x.RelativeFilename), x)));

    }

    private static List<Scenario> LoadScenarioDatabaseFile(string workshop_dbpath) {

        List<Scenario> workshopScenarios = new();
        if (File.Exists(workshop_dbpath)) {
            string rawJson = File.ReadAllText(workshop_dbpath);
            var scenarios = JsonSerializer.Deserialize<Scenario[]?>(rawJson);
            scenarios?.ForEach(x => workshopScenarios.Add(x));
        }

        return workshopScenarios;

    }

    public static bool IsValidMapDirectory(string x) {
        if (Directory.Exists(x)) {
            string[] d = Directory.GetDirectories(x);
            return d.Length == 1 && Directory.GetFiles(d[0]).Length > 0;
        } else {
            return false;
        }
    }

    private static void LoadNewWorkshopScenarios(List<Scenario> workshopScenarios, out int newWorkshopEntries) {

        // Bail if no scenario list
        if (__scenarios is null) {
            newWorkshopEntries = 0;
            return;
        }

        // Set new entry counter
        newWorkshopEntries = 0;

        // Get workshop filepath
        string workshopScenarioFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\Mods\\Scenarios\\subscriptions";

        // Verify it exists
        if (!Directory.Exists(workshopScenarioFolder)) {
            Trace.WriteLine("Failed to locate workshop folder. (CoH2 may never have been launched on this device).", "WorkshopScenarios");
            return;
        }

        // Get paths
        string extractPath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.EXTRACT_FOLDER);
        string extractReadPath = Path.Combine(extractPath, "scenarios");

        // Get sga files
        string[] files = Directory.GetFiles(workshopScenarioFolder, "*.sga");

        // Loop through all the .sga files in the subscriptions folder
        for (int i = 0; i < files.Length; i++) {

            string sga = Path.GetFileNameWithoutExtension(files[i]);
            if (!__scenarios.Any(x => x.Value.SgaName == sga) && !workshopScenarios.Any(x => x.SgaName == sga)) {

                if (!Archiver.Extract(files[i], extractPath + "\\")) {
                    Trace.WriteLine($"Failed to extract workshop scenario {sga}.", nameof(ScenarioList));
                }

                try {

                    // Get the directory to read from
                    string readfrom = $"{extractReadPath}\\mp\\community\\"
                        .IfTrue(IsValidMapDirectory)
                        .ElseIf($"{extractReadPath}\\mp\\", IsValidMapDirectory)
                        .ElseIf($"{extractReadPath}\\pm\\community\\", IsValidMapDirectory)
                        .ElseIf($"{extractReadPath}\\pm\\", IsValidMapDirectory)
                        .Else(extractReadPath, IsValidMapDirectory);

                    string[] dirs = Directory.GetDirectories(readfrom);
                    if (dirs.Length > 0) {
                        readfrom = dirs[0];

                        // Try and read scenario
                        if (GetScenarioFromDirectory(readfrom, sga, BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_USER_ICONS_FODLER)) is Scenario wrkscen) {

                            // Add scenario
                            workshopScenarios.Add(wrkscen);

                            // Increment new entries
                            newWorkshopEntries++;

                        }

                    } else {
                        Trace.WriteLine($"Failed to read sga \"{sga}\" (Skipping, unknown file structure)", nameof(ScenarioList));
                    }
                } catch (Exception e) {
                    Trace.WriteLine($"Failed to read sga \"{sga}\" (Skipping, message = \"{e.Message}\")", nameof(ScenarioList));
                }

            }

        }

    }

    public static Scenario? GetScenarioFromDirectory(string scenarioDirectoryPath, string sga = "MPScenarios.sga", string mmSavePath = "bin\\gfx\\map_icons\\") {

        // If valid scenario path
        if (Directory.Exists(scenarioDirectoryPath)) {

            // Create minimap save path if not found
            if (!Directory.Exists(mmSavePath)) {
                _ = Directory.CreateDirectory(mmSavePath);
            }

            // Get scenario files
            string[] scenarioFiles = Directory.GetFiles(scenarioDirectoryPath);
            string? info = scenarioFiles.FirstOrDefault(x => x.EndsWith(".info", false, CultureInfo.InvariantCulture));
            string? opt = scenarioFiles.FirstOrDefault(x => x.EndsWith(".options", false, CultureInfo.InvariantCulture));

            // Make sure there's actually a info file
            if (string.IsNullOrEmpty(info)) {
                return null;
            }

            // Make sure there's actually an options file
            if (string.IsNullOrEmpty(opt)) {
                return null;
            }

            // Define scenario variable
            Scenario? scen = null;

            try {

                // Create scenario
                scen = new Scenario(info, opt) {
                    SgaName = sga
                };

                // Find the index of the minimap
                int minimapFile = scenarioFiles.IndexOf(x => x.EndsWith("_preview.tga", false, CultureInfo.InvariantCulture))
                    .IfTrue(x => x == -1).Then(x => scenarioFiles.IndexOf(x => x.EndsWith("_mm.tga", false, CultureInfo.InvariantCulture)));

                // Make sure it's a valid index
                if (minimapFile != -1) {

                    // Save destination
                    string destination = $"{mmSavePath}{scen.RelativeFilename}_map.tga";

                    // Copy the found index file
                    File.Copy(scenarioFiles[minimapFile], destination, true);

                }

            } catch (LuaException lex) {
                Trace.WriteLine($"Failed to read scenario {sga}.sga (Lua Error): {lex.Message}", nameof(ScenarioList));
            } catch (Exception ex) {
                Trace.WriteLine($"Failed to read scenario {sga}.sga; {ex.Message}", nameof(ScenarioList));
            }

            // Delete the extracted files
            Directory.Delete(scenarioDirectoryPath, true);

            // Return the scenario
            return scen;

        } else {
            return null;
        }

    }

    /// <summary>
    /// Add a new <see cref="Scenario"/> to the scenario list.
    /// </summary>
    /// <param name="scenario">The new scenario to add.</param>
    /// <returns>Will return true if the <see cref="Scenario"/> was added. Otherwise false - <see cref="Scenario"/> already exists.</returns>
    public static bool AddScenario(Scenario scenario) {
        
        // Make sure we have something to add scenario to
        if (__scenarios is null)
            return false;

        // Try add scenario
        string relpath = Path.GetFileNameWithoutExtension(scenario.RelativeFilename);
        if (!__scenarios.ContainsKey(relpath)) {
            __scenarios.Add(relpath, scenario);
            return true;
        } else {
            return false;
        }

    }

    /// <summary>
    /// Get a <see cref="Scenario"/> from its file name.
    /// </summary>
    /// <param name="filename">The name of the scenario's file name to get.</param>
    /// <returns>The found scenario. If no matching scenario is found, an exception is thrown.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    public static Scenario? FromFilename(string filename) => __scenarios is not null ? __scenarios[filename] : null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static Scenario? FromRelativeFilename(string filename) => __scenarios?.FirstOrDefault(x => x.Value.RelativeFilename == filename).Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string ScenarioNameFromRelativeFilename(string filename) {
        if (FromRelativeFilename(filename) is Scenario scen) {
            return scen.Name;
        } else {
            return filename;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static bool TryFindScenario(string identifier, [NotNullWhen(true)] out Scenario? scenario) {
        scenario = null;
        if (__scenarios is null) {
            return false;
        }
        if (__scenarios.ContainsKey(identifier)) {
            scenario = __scenarios[identifier];
            return true;
        } else {
            scenario = FromRelativeFilename(identifier);
            return scenario is not null;
        }
    }

    public static List<Scenario> GetList() => __scenarios?.Values.Where(x => x.HasValidInfoOrOptionsFile).ToList() ?? new();

}
