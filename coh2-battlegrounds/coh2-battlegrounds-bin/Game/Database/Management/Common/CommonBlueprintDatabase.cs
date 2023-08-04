using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Management.Common;

/// <summary>
/// 
/// </summary>
public abstract class CommonBlueprintDatabase : IModBlueprintDatabase {

    /// <summary>
    /// 
    /// </summary>
    protected readonly HashSet<IModBlueprintDatabase> inheritsDatabase;

    /// <summary>
    /// 
    /// </summary>
    public CommonBlueprintDatabase() {
        inheritsDatabase = new();
    }

    /// <inheritdoc/>
    public abstract GameCase Game { get; }

    /// <inheritdoc/>
    public abstract void AddBlueprints(Array blueprints, BlueprintType blueprintType);

    /// <inheritdoc/>
    public abstract Blueprint FromBlueprintName(string id, BlueprintType bType);

    /// <inheritdoc/>
    public abstract Bp FromBlueprintName<Bp>(string bpName) where Bp : Blueprint;

    /// <inheritdoc/>
    public abstract T FromPPbgid<T>(BlueprintUID pBGID) where T : Blueprint;

    /// <inheritdoc/>
    public abstract IDictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type);

    /// <inheritdoc/>
    public abstract BlueprintCollection<T> GetCollection<T>() where T : Blueprint;

    /// <inheritdoc/>
    public virtual SquadBlueprint? GetCrewBlueprint(SquadBlueprint sbp, Faction? faction = null) {

        // Make sure there's actually a crew to get
        if (!sbp.HasCrew) {
            return null;
        }

        // Set faction
        faction ??= sbp.Army;

        // Bail if still null
        if (faction is not Faction f)
            return null;

        // Loop over entities in loadout
        for (int i = 0; i < sbp.Loadout.Count; i++) {

            // Get loadout entity and skip if null
            var e = sbp.Loadout.GetEntityName(i);
            if (e is null)
                continue;

            // Get entity
            var entity = FromBlueprintName<EntityBlueprint>(e);

            // Check for driver extension
            if (entity.Drivers is DriverExtension drivers) {
                var f2 = faction is null ? f : faction;
                var crew = drivers.Drivers.FirstOrDefault(x => x.Faction == f2.RbpPath).SquadBlueprint;
                return FromBlueprintName<SquadBlueprint>(crew);
            }

        }

        // No driver was found
        return null;

    }

    /// <inheritdoc/>
    public virtual void Inherit(IModBlueprintDatabase modBlueprintDatabase) {
        if (modBlueprintDatabase.Game != Game) {
            throw new NotSupportedException($"Cannot inherit from a '{modBlueprintDatabase.Game}' database when inheriter is for the game '{Game}'");
        }
        inheritsDatabase.Add(modBlueprintDatabase);
    }

}
