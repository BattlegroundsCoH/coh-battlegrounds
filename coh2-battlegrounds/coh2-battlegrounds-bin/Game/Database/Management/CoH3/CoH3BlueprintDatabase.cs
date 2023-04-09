using System;
using System.Collections.Generic;

using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Management.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3BlueprintDatabase : IModBlueprintDatabase {

    /// <inheritdoc/>
    public GameCase Game => GameCase.CompanyOfHeroes3;

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
