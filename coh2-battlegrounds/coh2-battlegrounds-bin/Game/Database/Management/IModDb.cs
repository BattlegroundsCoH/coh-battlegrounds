namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public interface IModDb {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    IModBlueprintDatabase GetBlueprints(GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    IWinconditionList GetWinconditions(GameCase game);

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
