using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Collections;
using Battlegrounds.Game.Database.Management.Common;
using Battlegrounds.Meta.Annotations;
using Battlegrounds.Util;

namespace Battlegrounds.Game.Database.Management.CoH3;

/// <summary>
/// 
/// </summary>
[GameSpecific(GameCase.CompanyOfHeroes3)]
public sealed class CoH3BlueprintDatabase : CommonBlueprintDatabase {

    private readonly SearchTree<Blueprint>? __entities;
    private readonly SearchTree<Blueprint>? __squads;
    private readonly SearchTree<Blueprint>? __abilities;
    private readonly SearchTree<Blueprint>? __upgrades;
    private readonly SearchTree<Blueprint>? __weapons;

    private readonly Dictionary<BlueprintType, HashSet<SearchTree<Blueprint>>> _inheritTrees;

    /// <inheritdoc/>
    public override GameCase Game => GameCase.CompanyOfHeroes3;

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

        // Create inherit trees
        _inheritTrees = new() {
            [BlueprintType.ABP] = new(),
            [BlueprintType.EBP] = new(),
            [BlueprintType.SBP] = new(),
            [BlueprintType.UBP] = new(),
            [BlueprintType.WBP] = new(),
        };

    }

    /// <inheritdoc/>
    public override void AddBlueprints(Array blueprints, BlueprintType blueprintType) {
        
        // Get target
        var target = GetSearchTree(blueprintType);

        // Iterate and add
        for (int i = 0; i < blueprints.Length; i++) {
            if (blueprints.GetValue(i) is Blueprint bp) {
                target.Add(bp.Name, bp);
            }
        }

    }

    /// <inheritdoc/>
    public override Bp FromBlueprintName<Bp>(string bpName)
        => FromBlueprintName(bpName, Blueprint.BlueprintTypeFromType<Bp>()) is Bp bp 
        ? bp : throw new InvalidBlueprintTypeException($"Blueprint '{bpName}' is not of type '{typeof(Bp).Name}'.");

    /// <inheritdoc/>
    public override Blueprint FromBlueprintName(string id, BlueprintType bType)
        => GetAllBlueprintsFromType(bType).Lookup(id) ?? throw new BlueprintNotFoundException(id);

    /// <inheritdoc/>
    public override T FromPPbgid<T>(BlueprintUID pBGID)
        => GetAllBlueprintsFromType(Blueprint.BlueprintTypeFromType<T>()).FirstOrDefault(x => x.PBGID == pBGID) is T bp
        ? bp : throw new BlueprintNotFoundException(pBGID.ToString());

    /// <inheritdoc/>
    public override IDictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type)
        => GetAllBlueprintsFromType(type).ToDictionary(x => x.PBGID);

    /// <inheritdoc/>
    public override BlueprintCollection<T> GetCollection<T>() {
        var src = GetAllBlueprintsOfType(Blueprint.BlueprintTypeFromType<T>());
        var mappedSrc = src.Map((k, v) => (T)v);
        return new BlueprintCollection<T>(mappedSrc);
    }

    private SearchTree<Blueprint> GetSearchTree(BlueprintType type) => type switch {
        BlueprintType.ABP => __abilities ?? throw new Exception(),
        BlueprintType.UBP => __upgrades ?? throw new Exception(),
        BlueprintType.EBP => __entities ?? throw new Exception(),
        BlueprintType.SBP => __squads ?? throw new Exception(),
        BlueprintType.WBP => __weapons ?? throw new Exception(),
        _ => throw new NotSupportedException(),
    };

    private SearchTree<Blueprint> GetAllBlueprintsFromType(BlueprintType type) => type switch {
        BlueprintType.ABP => __abilities?.Merge(_inheritTrees[BlueprintType.ABP].ToArray()) ?? throw new Exception(),
        BlueprintType.UBP => __upgrades?.Merge(_inheritTrees[BlueprintType.UBP].ToArray()) ?? throw new Exception(),
        BlueprintType.EBP => __entities?.Merge(_inheritTrees[BlueprintType.EBP].ToArray()) ?? throw new Exception(),
        BlueprintType.SBP => __squads?.Merge(_inheritTrees[BlueprintType.SBP].ToArray()) ?? throw new Exception(),
        BlueprintType.WBP => __weapons?.Merge(_inheritTrees[BlueprintType.WBP].ToArray()) ?? throw new Exception(),
        _ => throw new NotSupportedException(),
    };

    /// <inheritdoc/>
    public override void Inherit(IModBlueprintDatabase modBlueprintDatabase) {
        base.Inherit(modBlueprintDatabase);
        
        if (modBlueprintDatabase is CoH3BlueprintDatabase coh3bo) {
            _inheritTrees[BlueprintType.ABP].Add(coh3bo.__abilities!);
            _inheritTrees[BlueprintType.EBP].Add(coh3bo.__entities!);
            _inheritTrees[BlueprintType.SBP].Add(coh3bo.__squads!);
            _inheritTrees[BlueprintType.UBP].Add(coh3bo.__upgrades!);
            _inheritTrees[BlueprintType.WBP].Add(coh3bo.__weapons!);
        } 

    }

}
