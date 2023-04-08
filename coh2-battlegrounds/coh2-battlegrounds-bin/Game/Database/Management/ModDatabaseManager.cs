using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Database.Management.CoH2;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Vanilla;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public class ModDatabaseManager : IModDbManager {

    private readonly Dictionary<IModPackage, IModDb> packageDatabases;
    private readonly IModManager modManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modManager"></param>
    public ModDatabaseManager(IModManager modManager) {
        this.modManager = modManager;
        this.packageDatabases = new Dictionary<IModPackage, IModDb>();
    }

    /// <inheritdoc/>
    public IModBlueprintDatabase? GetBlueprints(IModPackage package, GameCase game) 
        => packageDatabases.TryGetValue(package, out var database) && database is not null ? database.GetBlueprints(game) : null;

    /// <inheritdoc/>
    public IModBlueprintDatabase GetBlueprintSource(Blueprint blueprint) {
        var package = packageDatabases.Keys.FirstOrDefault(x => x.TuningGUID == blueprint.PBGID.Mod) ?? throw new System.Exception("fff");
        return GetBlueprints(package, blueprint.Game)!;
    }

    /// <inheritdoc/>
    public IModLocale? GetLocale(IModPackage package, GameCase game)
        => packageDatabases.TryGetValue(package, out var database) && database is not null ? database.GetLocale(game) : null;

    /// <inheritdoc/>
    public IModLocale GetLocale(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => new CoH2Locale(),
        _ => throw new NotImplementedException()
    };

    /// <inheritdoc/>
    public IModLocale GetLocaleSource(Blueprint blueprint) {
        var package = packageDatabases.Keys.FirstOrDefault(x => x.TuningGUID == blueprint.PBGID.Mod) ?? throw new System.Exception("fff");
        return GetLocale(package, blueprint.Game)!;
    }

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

            // Create database
            ModDatabase database = new ModDatabase(modManager, package);
            
            // Load locale for mod (Skip vanilla packages)
            if (package is not VanillaModPackage) {
                database.LoadLocales();
            }

            var (successLoad, failLoad) = await database.LoadBlueprints(package.DataSourcePath);
            database.LoadWinconditions();
            database.LoadScenarios(() => { });

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
