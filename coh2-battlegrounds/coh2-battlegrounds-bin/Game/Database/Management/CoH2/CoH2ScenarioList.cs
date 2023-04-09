using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Game.Database.Management.CoH2;

/// <summary>
/// 
/// </summary>
public class CoH2ScenarioList : IScenarioList {

    private static readonly Logger logger = Logger.CreateLogger();
    private static readonly Dictionary<string, Scenario> __coh2Scenarios = LoadScenarios();

    private readonly Dictionary<string, Scenario> scenarios;

    /// <summary>
    /// 
    /// </summary>
    public CoH2ScenarioList() {
        this.scenarios = new Dictionary<string, Scenario>();
    }

    /// <inheritdoc/>
    public Scenario? FromFilename(string filename) => scenarios?[filename];

    /// <inheritdoc/>
    public Scenario? FromRelativeFilename(string filename) => scenarios?.FirstOrDefault(x => x.Value.RelativeFilename == filename).Value;

    /// <inheritdoc/>
    public IList<Scenario> GetList() => scenarios.Values.Union(__coh2Scenarios.Values).ToArray();

    /// <inheritdoc/>
    public Scenario? GetScenarioFromDirectory(string scenarioDirectoryPath, string sga = "MPScenarios.sga", string mmSavePath = "bin\\gfx\\map_icons\\") {

        // If valid scenario path
        if (Directory.Exists(scenarioDirectoryPath)) {

            // Create minimap save path if not found
            if (!Directory.Exists(mmSavePath)) {
                _ = Directory.CreateDirectory(mmSavePath);
            }

            // Get scenario files
            string[] scenarioFiles = Directory.GetFiles(scenarioDirectoryPath);
            string? lao = scenarioFiles.FirstOrDefault(x => x.EndsWith("_lao.dds", false, CultureInfo.InvariantCulture));
            string? info = scenarioFiles.FirstOrDefault(x => x.EndsWith(".info", false, CultureInfo.InvariantCulture));
            string? opt = scenarioFiles.FirstOrDefault(x => x.EndsWith(".options", false, CultureInfo.InvariantCulture));

            // Define scenario variable
            Scenario? scen = null;

            try {

                // Create scenario
                scen = Scenario.ReadScenario(lao, info, opt, sga);
                if (scen is null) {
                    throw new Exception("Failed to read scenario");
                }

                // Find the index of the minimap
                int minimapFile = scenarioFiles.IndexOf(x => x.EndsWithAny("_mm_high.tga", "_mm_low.tga"));
                if (minimapFile is -1) {
                    minimapFile = scenarioFiles.IndexOf(x => x.EndsWithAny("_mm.tga", "_preview.tga"));
                }

                // Make sure it's a valid index
                if (minimapFile != -1) {

                    // Save destination
                    string destination = $"{mmSavePath}{scen.RelativeFilename}_map.tga";

                    // Copy the found index file
                    File.Copy(scenarioFiles[minimapFile], destination, true);

                }

            } catch (Exception ex) {
                logger.Warning($"Failed to read scenario {sga}.sga; {ex.Message}");
            }

            // Delete the extracted files
            Directory.Delete(scenarioDirectoryPath, true);

            // Return the scenario
            return scen;

        } else {
            return null;
        }

    }

    private static bool IsValidMapDirectory(string x) {
        if (Directory.Exists(x)) {
            string[] d = Directory.GetDirectories(x);
            return d.Length == 1 && Directory.GetFiles(d[0]).Length > 0;
        } else {
            return false;
        }
    }

    /// <inheritdoc/>
    public void LoadWorkshopScenarios() {
        /*
        // Bail if no scenario list
        if (scenarios is null) {
            return;
        }

        // Get workshop filepath
        string workshopScenarioFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\Mods\\Scenarios\\subscriptions";

        // Verify it exists
        if (!Directory.Exists(workshopScenarioFolder)) {
            logger.Warning("Failed to locate workshop folder. (CoH2 may never have been launched on this device).");
            return;
        }

        // Get paths
        string extractPath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.EXTRACT_FOLDER);
        string extractReadPath = Path.Combine(extractPath, "scenarios");

        // Get sga files
        string[] files = Directory.GetFiles(workshopScenarioFolder, "*.sga");

        // Loop through all the .sga files in the subscriptions folder
        for (int i = 0; i < files.Length; i++) {

            string sga = Path.GetFileNameWithoutExtension(files[i]);
            if (!scenarios.Any(x => x.Value.SgaName == sga) && !workshopScenarios.Any(x => x.SgaName == sga)) {

                if (!Archiver.Extract(files[i], extractPath + "\\")) {
                    logger.Warning($"Failed to extract workshop scenario {sga}.");
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
                        if (GetScenarioFromDirectory(readfrom, sga, BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_USER_ICONS_FODLER)) is Scenario wrkscen) {

                            // Add scenario
                            workshopScenarios.Add(wrkscen);

                        }

                    } else {
                        logger.Warning($"Failed to read sga \"{sga}\" (Skipping, unknown file structure)");
                    }
                } catch (Exception e) {
                    logger.Warning($"Failed to read sga \"{sga}\" (Skipping, message = \"{e.Message}\")");
                }

            }

        }
        */
    }

    /// <inheritdoc/>
    public bool RegisterScenario(Scenario scenario) {

        // Try add scenario
        string relpath = Path.GetFileNameWithoutExtension(scenario.RelativeFilename);
        if (!scenarios.ContainsKey(relpath)) {
            scenarios.Add(relpath, scenario);
            return true;
        } else {
            return false;
        }

    }

    /// <inheritdoc/>
    public bool TryFindScenario(string identifier, [NotNullWhen(true)] out Scenario? scenario) {
        scenario = null;
        if (scenarios.ContainsKey(identifier)) {
            scenario = scenarios[identifier];
            return true;
        } else {
            scenario = FromRelativeFilename(identifier);
            return scenario is not null;
        }
    }

    private static Dictionary<string, Scenario> LoadScenarios() {

        Dictionary<string, Scenario> scenarios = new Dictionary<string, Scenario>();

        string rawJsonDb = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.DATABASE_FOLDER, "vcoh2-map-db.json");
        IScenarioList.LoadScenarioDatabaseFile(rawJsonDb).ForEach(x => {
            if (!scenarios.TryAdd(Path.GetFileNameWithoutExtension(x.RelativeFilename), x)) {
                logger.Warning($"Failed to add duplicate vcoh scenario '{x.RelativeFilename}'");
            }
        });

        return scenarios;

    }

}
