using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Game.Scenarios.CoH2;
using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Game.Database.Management.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3ScenarioList : IScenarioList {

    private static readonly Logger logger = Logger.CreateLogger();
    private static readonly Dictionary<string, IScenario> __coh3Scenarios = LoadScenarios();

    private readonly Dictionary<string, IScenario> scenarios;

    /// <summary>
    /// 
    /// </summary>
    public CoH3ScenarioList() {
        this.scenarios = new Dictionary<string, IScenario>();
    }

    /// <inheritdoc/>
    public IScenario? FromFilename(string filename) => scenarios?[filename];

    /// <inheritdoc/>
    public IScenario? FromRelativeFilename(string filename) => scenarios?.FirstOrDefault(x => x.Value.RelativeFilename == filename).Value;

    /// <inheritdoc/>
    public IList<IScenario> GetList() => scenarios.Values.Union(__coh3Scenarios.Values).ToArray();

    /// <inheritdoc/>
    public IScenario? GetScenarioFromDirectory(string scenarioDirectoryPath, string sga = "ScenariosMP.sga", string mmSavePath = "bin\\gfx\\map_icons\\") {

        // Ensure valid directory
        if (!Directory.Exists(scenarioDirectoryPath)) {
            logger.Warning("Failed getting scenario from directory: " + scenarioDirectoryPath + " (Path not found");
            return null;
        }

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
        CoH2Scenario? scen = null;

        try {

            // Create scenario
            scen = CoH2Scenario.ReadScenario(lao, info, opt, sga);
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
            logger.Warning($"Failed to read scenario {sga}; {ex.Message}");
        }

        // Delete the extracted files
        //Directory.Delete(scenarioDirectoryPath, true);

        // Return the scenario
        return scen;

    }

    /// <inheritdoc/>
    public void LoadWorkshopScenarios() {
    }

    /// <inheritdoc/>
    public bool RegisterScenario(IScenario scenario) {

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
    public bool TryFindScenario(string identifier, [NotNullWhen(true)] out IScenario? scenario) {
        if (scenarios.ContainsKey(identifier)) {
            scenario = scenarios[identifier];
            return true;
        } else {
            scenario = FromRelativeFilename(identifier);
            return scenario is not null;
        }
    }

    private static Dictionary<string, IScenario> LoadScenarios() {

        Dictionary<string, IScenario> scenarios = new Dictionary<string, IScenario>();

        string rawJsonDb = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.DATABASE_FOLDER, "vcoh3-map-db.json");
        IScenarioList.LoadScenarioDatabaseFile(rawJsonDb).ForEach(x => {
            if (!scenarios.TryAdd(Path.GetFileNameWithoutExtension(x.RelativeFilename), x)) {
                logger.Warning($"Failed to add duplicate vcoh scenario '{x.RelativeFilename}'");
            }
        });

        return scenarios;

    }

}
