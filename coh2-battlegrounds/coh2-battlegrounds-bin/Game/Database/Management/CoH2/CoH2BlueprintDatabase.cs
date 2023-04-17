using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Database.Management.Common;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management.CoH2;

/// <summary>
/// 
/// </summary>
public sealed class CoH2BlueprintDatabase : CommonBlueprintDatabase {

    private readonly Dictionary<BlueprintUID, Blueprint>? __entities;
    private readonly Dictionary<BlueprintUID, Blueprint>? __squads;
    private readonly Dictionary<BlueprintUID, Blueprint>? __abilities;
    private readonly Dictionary<BlueprintUID, Blueprint>? __upgrades;
    private readonly Dictionary<BlueprintUID, Blueprint>? __criticals;
    private readonly Dictionary<BlueprintUID, Blueprint>? __slotitems;
    private readonly Dictionary<BlueprintUID, Blueprint>? __weapons;

    private ushort bpCntr = 0;

    /// <inheritdoc/>
    public override GameCase Game => GameCase.CompanyOfHeroes2;

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

    }

    /// <inheritdoc/>
    public override void AddBlueprints(Array blueprints, BlueprintType blueprintType) {

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
    public override BlueprintCollection<T> GetCollection<T>() 
        => new BlueprintCollection<T>(GetAllBlueprintsOfType(Blueprint.BlueprintTypeFromType<T>()).Map((k,v) => (T)v));

    /// <inheritdoc/>
    public override Blueprint FromBlueprintName(string id, BlueprintType bType) {
        if (GetModGUID(id, out ModGuid guid, out string bp)) {
            return GetAllBlueprintsOfType(bType).FirstOrDefault(x => x.Value?.PBGID.Mod == guid && x.Value?.Name == bp).Value;
        }
        return GetAllBlueprintsOfType(bType).FirstOrDefault(x => x.Value?.Name == id).Value;
    }

    /// <inheritdoc/>
    public override Bp FromBlueprintName<Bp>(string bpName)
        => (Bp)FromBlueprintName(bpName, Blueprint.BlueprintTypeFromType<Bp>());

    /// <inheritdoc/>
    public override IDictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type) => type switch {
        BlueprintType.ABP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__abilities!, (src, next) => src.Union(next)),
        BlueprintType.CBP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__criticals!, (src, next) => src.Union(next)),
        BlueprintType.EBP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__entities!, (src, next) => src.Union(next)),
        BlueprintType.SBP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__squads!, (src, next) => src.Union(next)),
        BlueprintType.UBP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__upgrades!, (src, next) => src.Union(next)),
        BlueprintType.IBP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__slotitems!, (src, next) => src.Union(next)),
        BlueprintType.WBP => inheritsDatabase.Select(x => x.GetAllBlueprintsOfType(type)).Aggregate((IDictionary<BlueprintUID, Blueprint>)__weapons!, (src, next) => src.Union(next)),
        _ => throw new ArgumentException(null, nameof(type)),
    };

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
    public override T FromPPbgid<T>(BlueprintUID pBGID)
        => (T)GetAllBlueprintsOfType(Blueprint.BlueprintTypeFromType<T>())[pBGID];

}
