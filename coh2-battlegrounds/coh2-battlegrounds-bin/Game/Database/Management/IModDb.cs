namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// Interface for handling the various databases in use by a modification.
/// </summary>
public interface IModDb {

    /// <summary>
    /// Get the mod blueprints for the specified game.
    /// </summary>
    /// <param name="game">The game to get blueprints for.</param>
    /// <returns>The <see cref="IModBlueprintDatabase"/> instance containing blueprint instances registered for the mod.</returns>
    IModBlueprintDatabase GetBlueprints(GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    IModBlueprintDatabase? GetBlueprintsOrNull(GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    IGamemodeList GetGamemodes(GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    IModLocale GetLocale(GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    IScenarioList GetScenarios(GameCase game);

}
