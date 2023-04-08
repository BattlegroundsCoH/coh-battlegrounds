using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.ErrorHandling;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// 
/// </summary>
public sealed class CoH2BlueprintDatabase : IModBlueprintDatabase {

    private readonly Dictionary<BlueprintUID, Blueprint>? __entities;
    private readonly Dictionary<BlueprintUID, Blueprint>? __squads;
    private readonly Dictionary<BlueprintUID, Blueprint>? __abilities;
    private readonly Dictionary<BlueprintUID, Blueprint>? __upgrades;
    private readonly Dictionary<BlueprintUID, Blueprint>? __criticals;
    private readonly Dictionary<BlueprintUID, Blueprint>? __slotitems;
    private readonly Dictionary<BlueprintUID, Blueprint>? __weapons;

    private readonly List<Dictionary<BlueprintUID, Blueprint>>? __selfList;

    private ushort bpCntr = 0;

    /// <summary>
    /// 
    /// </summary>
    public CoH2BlueprintDatabase() {

        // Create Dictionaries
        __entities = new();
        __squads = new();
        __abilities = new();
        __upgrades = new();
        __criticals = new();
        __slotitems = new();
        __weapons = new();

        // Create list over self
        __selfList = new List<Dictionary<BlueprintUID, Blueprint>>() {
                __abilities,
                __criticals,
                __entities,
                __slotitems,
                __squads,
                __upgrades,
                __weapons,
            };

    }

    /// <inheritdoc/>
    public void AddBlueprints(Array blueprints, BlueprintType blueprintType) {

        // Get target array
        var target = GetAllBlueprintsOfType(blueprintType);

        // Loop over blueprints
        for (int i = 0; i < blueprints.Length; i++) {
            if (blueprints.GetValue(i) is Blueprint bp) {
                bp.ModPBGID = bpCntr++;
                target.Add(bp.PBGID, bp);
            }
        }

    }

    /// <inheritdoc/>
    public BlueprintCollection<T> GetCollection<T>() where T : Blueprint => new BlueprintCollection<T>(GetAllBlueprintsOfType(Blueprint.BlueprintTypeFromType<T>()));

    /// <inheritdoc/>
    public Blueprint FromBlueprintName(string id, BlueprintType bType) {
        if (GetModGUID(id, out ModGuid guid, out string bp)) {
            return GetAllBlueprintsOfType(bType).FirstOrDefault(x => x.Value?.PBGID.Mod == guid && x.Value?.Name == bp).Value;
        }
        return GetAllBlueprintsOfType(bType).FirstOrDefault(x => x.Value?.Name == id).Value;
    }

    /// <inheritdoc/>
    public Bp FromBlueprintName<Bp>(string bpName) where Bp : Blueprint
        => (Bp)FromBlueprintName(bpName, Blueprint.BlueprintTypeFromType<Bp>());

    /// <summary>
    /// Get all blueprints in database of <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of blueprint to retrieve.</param>
    /// <returns>Dictionary of <see cref="Blueprint"/> instances linked with their <see cref="BlueprintUID"/> keys.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="EnvironmentException"/>
    public Dictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type) => (type switch {
        BlueprintType.ABP => __abilities,
        BlueprintType.CBP => __criticals,
        BlueprintType.EBP => __entities,
        BlueprintType.SBP => __squads,
        BlueprintType.UBP => __upgrades,
        BlueprintType.IBP => __slotitems,
        BlueprintType.WBP => __weapons,
        _ => throw new ArgumentException(null, nameof(type)),
    }) ?? throw new EnvironmentException("Fatal error: Database not instantiated.");

    /// <summary>
    /// Retrieve the <see cref="ModGuid"/> from a string fully qualified blueprint name.
    /// </summary>
    /// <param name="bpname">The full blueprint name.</param>
    /// <param name="modGuid">The stored mod GUID in the blueprint name.</param>
    /// <param name="bp">The blueprint that follows the mod guid.</param>
    /// <returns>If extraction of both values was successful, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public static bool GetModGUID(string bpname, out ModGuid modGuid, out string bp) {
        int j = bpname.IndexOf(':');
        if (j == ModGuid.FIXED_LENGTH) {
            modGuid = ModGuid.FromGuid(bpname[0..j]);
            bp = bpname[(j + 1)..];
            return true;
        } else {
            modGuid = ModGuid.FromGuid(Guid.Empty);
            bp = bpname;
            return false;
        }
    }

    /// <inheritdoc/>
    public T FromPPbgid<T>(BlueprintUID pBGID) where T : Blueprint
        => (T)GetAllBlueprintsOfType(Blueprint.BlueprintTypeFromType<T>())[pBGID];

    /// <inheritdoc/>
    public SquadBlueprint? GetCrewBlueprint(SquadBlueprint sbp, Faction? faction = null) {

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
                var crew =  drivers.Drivers.FirstOrDefault(x => x.Faction == f2.RbpPath).SquadBlueprint;
                return FromBlueprintName<SquadBlueprint>(crew);
            }

        }

        // No driver was found
        return null;

    }

}
