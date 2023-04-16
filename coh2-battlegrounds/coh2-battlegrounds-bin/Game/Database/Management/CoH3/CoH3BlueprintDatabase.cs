using System;
using System.Collections.Generic;

using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Util;

namespace Battlegrounds.Game.Database.Management.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3BlueprintDatabase : IModBlueprintDatabase {

    private readonly SearchTree<Blueprint>? __entities;
    private readonly SearchTree<Blueprint>? __squads;
    private readonly SearchTree<Blueprint>? __abilities;
    private readonly SearchTree<Blueprint>? __upgrades;
    private readonly SearchTree<Blueprint>? __weapons;

    private readonly HashSet<IModBlueprintDatabase> __inherit;

    /// <inheritdoc/>
    public GameCase Game => GameCase.CompanyOfHeroes3;

    /// <summary>
    /// 
    /// </summary>
    public CoH3BlueprintDatabase() {

        // Create Dictionaries
        __entities = new();
        __squads = new();
        __abilities = new();
        __upgrades = new();
        __weapons = new();

        // Create list of inheritance blueprints
        __inherit = new();

    }

    /// <inheritdoc/>
    public void AddBlueprints(Array blueprints, BlueprintType blueprintType) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Bp FromBlueprintName<Bp>(string bpName) where Bp : Blueprint {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Blueprint FromBlueprintName(string id, BlueprintType bType) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public T FromPPbgid<T>(BlueprintUID pBGID) where T : Blueprint {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IDictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public BlueprintCollection<T> GetCollection<T>() where T : Blueprint {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public SquadBlueprint? GetCrewBlueprint(SquadBlueprint sbp, Faction? faction = null) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Inherit(IModBlueprintDatabase modBlueprintDatabase) {
        throw new NotImplementedException();
    }

}
