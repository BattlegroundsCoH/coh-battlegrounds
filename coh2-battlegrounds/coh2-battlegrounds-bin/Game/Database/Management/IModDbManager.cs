using Battlegrounds.Game.Blueprints;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// Callback handler for the <see cref="IModDbManager"/> being done loading databases.
/// </summary>
/// <param name="db_loaded">The amount of databases that were loaded.</param>
/// <param name="db_failed">The amount of databases that failed to load.</param>
public delegate void DatabaseLoadedCallbackHandler(int db_loaded, int db_failed);

/// <summary>
/// Callback handler for handling the on load event
/// </summary>
public delegate void DatabaseLoadedHandler();

/// <summary>
/// 
/// </summary>
public interface IModDbManager {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    void LoadDatabases(DatabaseLoadedCallbackHandler callback);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="package"></param>
    /// <param name="game"></param>
    /// <returns></returns>
    IModBlueprintDatabase? GetBlueprints(IModPackage package, GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blueprint"></param>
    /// <returns></returns>
    IModBlueprintDatabase GetBlueprintSource(Blueprint blueprint);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="package"></param>
    /// <param name="game"></param>
    /// <returns></returns>
    IWinconditionList? GetWinconditionList(IModPackage package, GameCase game);

}
