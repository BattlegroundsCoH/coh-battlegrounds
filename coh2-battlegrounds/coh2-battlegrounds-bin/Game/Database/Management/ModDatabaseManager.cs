using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Database.Management.CoH2;
using Battlegrounds.Game.Database.Management.CoH3;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Vanilla;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public class ModDatabaseManager : IModDbManager {

    private readonly Dictionary<IModPackage, IModDb> packageDatabases;
    private readonly IModManager modManager;

    private readonly CoH2Locale coh2Locale;
    private readonly CoH3Locale coh3Locale;

    private readonly CoH2ScenarioList coh2Scenarios;
    private readonly CoH3ScenarioList coh3Scenarios;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modManager"></param>
    public ModDatabaseManager(IModManager modManager) {
    
        // Set internals
        this.modManager = modManager;
        this.packageDatabases = new Dictionary<IModPackage, IModDb>();
    
        // Prepare CoH2 defaults
        this.coh2Locale = new CoH2Locale();
        this.coh2Scenarios = new CoH2ScenarioList();

        // Prepare CoH3 defaults
        this.coh3Locale = new CoH3Locale();
        this.coh3Scenarios = new CoH3ScenarioList();

    }

    /// <inheritdoc/>
    public IModBlueprintDatabase? GetBlueprints(IModPackage package, GameCase game) 
        => packageDatabases.TryGetValue(package, out var database) && database is not null ? database.GetBlueprints(game) : null;

    /// <inheritdoc/>
    public IModBlueprintDatabase GetBlueprintSource(Blueprint blueprint) {
        var package = packageDatabases.Keys.Where(x => x.SupportedGames.HasFlag(blueprint.Game))
            .FirstOrDefault(x => x.TuningGUID == blueprint.PBGID.Mod) ?? throw new Exception("fff");
        return GetBlueprints(package, blueprint.Game)!;
    }

    /// <inheritdoc/>
    public IModLocale? GetLocale(IModPackage package, GameCase game)
        => packageDatabases.TryGetValue(package, out var database) && database is not null ? database.GetLocale(game) : null;

    /// <inheritdoc/>
    public IModLocale GetLocale(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => coh2Locale,
        GameCase.CompanyOfHeroes3 => coh3Locale,
        _ => throw new NotSupportedException()
    };

    /// <inheritdoc/>
    public IModLocale GetLocaleSource(Blueprint blueprint) {
        var package = packageDatabases.Keys.Where(x => x.SupportedGames.HasFlag(blueprint.Game))
            .FirstOrDefault(x => x.TuningGUID == blueprint.PBGID.Mod) ?? throw new Exception("fff");
        return GetLocale(package, blueprint.Game)!;
    }

    /// <inheritdoc/>
    public IScenarioList GetScenarioList(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => coh2Scenarios,
        GameCase.CompanyOfHeroes3 => coh3Scenarios,
        _ => throw new NotSupportedException()
    };

    /// <inheritdoc/>
    public IScenarioList? GetScenarioList(IModPackage package, GameCase game)
        => packageDatabases.TryGetValue(package, out var database) && database is not null ? database.GetScenarios(game) : null;

    /// <inheritdoc/>
    public IWinconditionList? GetWinconditionList(IModPackage package, GameCase game)
        => packageDatabases.TryGetValue(package, out var database) && database is not null ? database.GetWinconditions(game) : null;

    /// <inheritdoc/>
    public void LoadDatabases(DatabaseLoadedCallbackHandler callback) => Task.Run(async () => {

        // Get packages
        IList<IModPackage> packages = BattlegroundsContext.ModManager.GetPackages();

        // Keep track of load status
        int dbLoaded = 0, dbFailed = 0;

        // Iterate the packages
        for (int i = 0; i < packages.Count; i++) {

            // Get the package
            var package = packages[i];

            // Test if vanilla
            var isVanilla = package is VanillaModPackage;

            // Create database
            ModDatabase database = new ModDatabase(modManager, package);
            
            // Load locale for mod (Skip vanilla packages)
            if (!isVanilla) {
                database.LoadLocales();
            }

            // Load blueprints
            var (successLoad, failLoad) = await database.LoadBlueprints(package.DataSourcePath);
            if (!isVanilla) {
                database.GetBlueprintsOrNull(GameCase.CompanyOfHeroes2)
                    ?.Inherit(packageDatabases[BattlegroundsContext.ModManager.GetVanillaPackage(GameCase.CompanyOfHeroes2)].GetBlueprints(GameCase.CompanyOfHeroes2));
                var coh3Blueprints = packageDatabases[BattlegroundsContext.ModManager.GetVanillaPackage(GameCase.CompanyOfHeroes3)].GetBlueprints(GameCase.CompanyOfHeroes3);
                database.GetBlueprintsOrNull(GameCase.CompanyOfHeroes3)?.Inherit(coh3Blueprints);
            }

            // Load win conditions
            database.LoadWinconditions();

            // Load scenarios
            int scenarios = database.LoadScenarios(package.DataSourcePath);

            // Update loaded
            dbLoaded += successLoad;
            dbFailed += failLoad;

            // Store reference to database
            packageDatabases[package] = database;

            // Set the datasource
            package.SetDataSource(database);

        }

        // Invoke callback
        callback(dbLoaded, dbFailed);

    });

}
