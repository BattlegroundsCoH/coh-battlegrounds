using System;
using System.Collections.Generic;

using Battlegrounds.Game;

namespace Battlegrounds.Modding;

/// <summary>
/// Interface for a class managing several mods within a Battlegrounds application context.
/// </summary>
public interface IModManager {

    /// <summary>
    /// Initialise the <see cref="IModManager"/> and load available <see cref="IModPackage"/> elements.
    /// </summary>
    void LoadMods();

    /// <summary>
    /// Get package from its <paramref name="packageID"/>.
    /// </summary>
    /// <param name="packageID">The ID to use to identify the <see cref="IModPackage"/>.</param>
    /// <returns>The <see cref="IModPackage"/> associated with <paramref name="packageID"/>.</returns>
    IModPackage? GetPackage(string packageID);

    /// <summary>
    /// Get package from its <paramref name="packageID"/>.
    /// </summary>
    /// <param name="packageID">The ID to use to identify the <see cref="IModPackage"/>.</param>
    /// <returns>The <see cref="IModPackage"/> associated with <paramref name="packageID"/>.</returns>
    IModPackage GetPackageOrError(string packageID);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameCase"></param>
    /// <returns></returns>
    IModPackage GetVanillaPackage(GameCase gameCase);

    /// <summary>
    /// Iterate over each <see cref="IModPackage"/> in the system.
    /// </summary>
    /// <param name="modPackageAction">The action to invoke with each package element.</param>
    void EachPackage(Action<IModPackage> modPackageAction);

    /// <summary>
    /// Get the abstract <see cref="IGameMod"/> instance represented by <paramref name="guid"/>.
    /// </summary>
    /// <typeparam name="TMod">The specifc <see cref="IGameMod"/> type to get.</typeparam>
    /// <param name="guid">The GUID of the mod to fetch.</param>
    /// <returns>The <see cref="IGameMod"/> instance associated with the <paramref name="guid"/>.</returns>
    TMod? GetMod<TMod>(ModGuid guid) where TMod : class, IGameMod;

    /// <summary>
    /// Get a <see cref="IModPackage"/> based on one of its submod <paramref name="guid"/> elements.
    /// </summary>
    /// <param name="guid">The <see cref="ModGuid"/> to get <see cref="IModPackage"/> from.</param>
    /// <param name="game">The game to find package in</param>
    /// <returns>The <see cref="IModPackage"/> associated with the submod associated <paramref name="guid"/>.</returns>
    IModPackage? GetPackageFromGuid(ModGuid guid, GameCase game);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IList<IModPackage> GetPackages();

}
