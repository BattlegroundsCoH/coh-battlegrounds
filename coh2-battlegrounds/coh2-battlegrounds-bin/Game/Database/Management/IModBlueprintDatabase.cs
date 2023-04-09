using System;
using System.Collections.Generic;

using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public interface IModBlueprintDatabase {

    GameCase Game { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blueprints"></param>
    /// <param name="blueprintType"></param>
    void AddBlueprints(Array blueprints, BlueprintType blueprintType);
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    BlueprintCollection<T> GetCollection<T>() where T : Blueprint;

    /// <summary>
    /// Get a <see cref="Blueprint"/> instance from its string name (file name).
    /// </summary>
    /// <param name="id">The string ID to look for in sub-databases.</param>
    /// <param name="bType">The <see cref="BlueprintType"/> to look for when looking up the <see cref="Blueprint"/>.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
    Blueprint FromBlueprintName(string id, BlueprintType bType);

    /// <summary>
    /// Get a <see cref="Blueprint"/> instance from its string name (file name).
    /// </summary>
    /// <typeparam name="Bp">The specific <see cref="Blueprint"/> type to fetch.</typeparam>
    /// <param name="bpName">The string ID to look for.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
    Bp FromBlueprintName<Bp>(string bpName) where Bp : Blueprint;

    /// <summary>
    /// Retrieve a <typeparamref name="T"/> instance from the blueprint database based on the given <see cref="BlueprintUID"/>.
    /// </summary>
    /// <typeparam name="T">The specific blueprint type.</typeparam>
    /// <param name="pBGID">The blueprint used to identify a blueprint.</param>
    /// <returns>The blueprint associated with the given id.</returns>
    T FromPPbgid<T>(BlueprintUID pBGID) where T : Blueprint;

    /// <summary>
    /// Get the <see cref="SquadBlueprint"/> that is the crew of the <see cref="SquadBlueprint"/>
    /// </summary>
    /// <param name="sbp">The vehicle blueprint to find crew for.</param>
    /// <param name="faction">The faction to get crew SBP from.</param>
    /// <returns>The crew <see cref="SquadBlueprint"/>. If not found for <paramref name="faction"/>, the default crew is returned.</returns>
    SquadBlueprint? GetCrewBlueprint(SquadBlueprint sbp, Faction? faction = null);
    
    /// <summary>
    /// Get all blueprints in database of <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of blueprint to retrieve.</param>
    /// <returns>Dictionary of <see cref="Blueprint"/> instances linked with their <see cref="BlueprintUID"/> keys.</returns>
    /// <exception cref="ArgumentException"/>
    IDictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type);

    /// <summary>
    /// Inherit blueprints from the input <see cref="IModBlueprintDatabase"/>.
    /// </summary>
    /// <param name="modBlueprintDatabase">The database to inherit blueprints from.</param>
    void Inherit(IModBlueprintDatabase modBlueprintDatabase);

}
