using System.Collections.Generic;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public interface IWinconditionList {

    /// <summary>
    /// Register a new <see cref="IGamemode"/> in the database.
    /// </summary>
    /// <param name="wincondition"></param>
    void AddWincondition(IGamemode wincondition);

    /// <summary>
    /// Get a list of all valid <see cref="IGamemode"/> instances for specified <paramref name="modGuid"/>.
    /// </summary>
    /// <param name="modGuid">The targetted mod.</param>
    /// <param name="captureBaseGame">Allow vanilla gamemodes</param>
    /// <returns>A list of <see cref="IGamemode"/> instances for <paramref name="modGuid"/>.</returns>
    List<IGamemode> GetGamemodes(ModGuid modGuid, bool captureBaseGame = false);

    /// <summary>
    /// Get a <see cref="IGamemode"/> by its gamemode identifier name.
    /// </summary>
    /// <param name="modGuid">The targetted mod.</param>
    /// <param name="gamemodeName">The name of the gamemode to get.</param>
    /// <returns>The found gamemode or null if not full.</returns>
    IGamemode? GetGamemodeByName(ModGuid modGuid, string gamemodeName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modGuid"></param>
    /// <param name="gamemodeNames"></param>
    /// <returns></returns>
    List<IGamemode> GetGamemodes(ModGuid modGuid, IEnumerable<string> gamemodeNames);


}
