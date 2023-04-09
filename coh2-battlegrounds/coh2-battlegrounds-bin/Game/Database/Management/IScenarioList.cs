using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

using Battlegrounds.Functional;
using Battlegrounds.Game.Scenarios;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public interface IScenarioList {

    /// <summary>
    /// 
    /// </summary>
    void LoadWorkshopScenarios();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scenarioDirectoryPath"></param>
    /// <param name="sga"></param>
    /// <param name="mmSavePath"></param>
    /// <returns></returns>
    Scenario? GetScenarioFromDirectory(string scenarioDirectoryPath, string sga = "MPScenarios.sga", string mmSavePath = "bin\\gfx\\map_icons\\");

    /// <summary>
    /// Register a new <see cref="Scenario"/> to the scenario list.
    /// </summary>
    /// <param name="scenario">The new scenario to add.</param>
    /// <returns>Will return true if the <see cref="Scenario"/> was added. Otherwise false - <see cref="Scenario"/> already exists.</returns>
    bool RegisterScenario(Scenario scenario);

    /// <summary>
    /// Get a <see cref="Scenario"/> from its file name.
    /// </summary>
    /// <param name="filename">The name of the scenario's file name to get.</param>
    /// <returns>The found scenario. If no matching scenario is found, an exception is thrown.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    Scenario? FromFilename(string filename);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    Scenario? FromRelativeFilename(string filename);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    bool TryFindScenario(string identifier, [NotNullWhen(true)] out Scenario? scenario);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IList<Scenario> GetList();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workshop_dbpath"></param>
    /// <returns></returns>
    public static IList<Scenario> LoadScenarioDatabaseFile(string workshop_dbpath) {

        // Create container
        List<Scenario> dbScenarios = new();

        // Check if file exists
        if (File.Exists(workshop_dbpath)) {
            string rawJson = File.ReadAllText(workshop_dbpath);
            var scenarios = JsonSerializer.Deserialize<Scenario[]?>(rawJson);
            scenarios?.ForEach(dbScenarios.Add);
        }

        // Return found scenarios
        return dbScenarios;

    }

}
