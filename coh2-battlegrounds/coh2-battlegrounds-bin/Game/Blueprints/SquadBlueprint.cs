using System;
using System.Linq;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Enum describing the category of a <see cref="SquadBlueprint"/>.
/// </summary>
public enum SquadCategory {

    /// <summary>
    /// Infantry category.
    /// </summary>
    Infantry,

    /// <summary>
    /// Support weapon/vehicle category (aka team weapon). Also includes support vehicles such as supply trucks.
    /// </summary>
    Support,

    /// <summary>
    /// Vehicle category.
    /// </summary>
    Vehicle,

    /// <summary>
    /// Leader category
    /// </summary>
    Leader,

}

/// <summary>
/// Representation of a <see cref="Blueprint"/> with <see cref="Squad"/> specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inherited.
/// </summary>
public sealed class SquadBlueprint : Blueprint, IUIBlueprint {

    /// <summary>
    /// Invalid squad blueprint instance with no data associated with it.
    /// </summary>
    public static readonly SquadBlueprint Invalid =
        new SquadBlueprint("sbp", new BlueprintUID(), null,
            new(),
            new(),
            new(Array.Empty<LoadoutExtension.Entry>()),
            new(Array.Empty<VeterancyExtension.Rank>()),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(), 0, 0, false, false, false, 0);

    /// <summary>
    /// Get the unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    /// <summary>
    /// Get the blueprint type (Is <see cref="BlueprintType.SBP"/>).
    /// </summary>
    public override BlueprintType BlueprintType => BlueprintType.SBP;

    /// <summary>
    /// Get the name of the <see cref="SquadBlueprint"/> instance.
    /// </summary>
    public override string Name { get; }

    /// <summary>
    /// Get the army the <see cref="SquadBlueprint"/> can be used by.
    /// </summary>
    public Faction? Army { get; }

    /// <summary>
    /// Get the UI extension.
    /// </summary>
    public UIExtension UI { get; }

    /// <summary>
    /// The base <see cref="CostExtension"/> to field instances of the <see cref="SquadBlueprint"/>.
    /// </summary>
    public CostExtension Cost { get; }

    /// <summary>
    /// Get the veterancy extension.
    /// </summary>
    public VeterancyExtension Veterancy { get; }

    /// <summary>
    /// Get the loadout extension.
    /// </summary>
    public LoadoutExtension Loadout { get; }

    /// <summary>
    /// Does the squad the bluperint is for, require a crew.
    /// </summary>
    public bool HasCrew { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsTeamWeapon { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool CanPickupItems { get; }

    /// <summary>
    /// 
    /// </summary>
    public int PickupCapacity { get; }

    /// <summary>
    /// 
    /// </summary>
    public string[] Abilities { get; }

    /// <summary>
    /// 
    /// </summary>
    public string[] Upgrades { get; }

    /// <summary>
    /// 
    /// </summary>
    public string[] AppliedUpgrades { get; }

    /// <summary>
    /// 
    /// </summary>
    public int UpgradeCapacity { get; }

    /// <summary>
    /// 
    /// </summary>
    public float FemaleSquadChance { get; }

    /// <summary>
    /// Array of types bound to the <see cref="SquadBlueprint"/>.
    /// </summary>
    public TypeList Types { get; }

    /// <summary>
    /// 
    /// </summary>
    public SquadCategory Category => GetCategory(Types);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pbgid"></param>
    /// <param name="faction"></param>
    /// <param name="ui"></param>
    /// <param name="cost"></param>
    /// <param name="loadout"></param>
    /// <param name="veterancy"></param>
    /// <param name="types"></param>
    /// <param name="abilities"></param>
    /// <param name="upgrades"></param>
    /// <param name="appliedUpgrades"></param>
    /// <param name="upgradeCapacity"></param>
    /// <param name="slotCapacity"></param>
    /// <param name="canPickup"></param>
    /// <param name="isTeamWpn"></param>
    /// <param name="hasCrew"></param>
    /// <param name="femaleChance"></param>
    public SquadBlueprint(string name, BlueprintUID pbgid, Faction? faction,
        UIExtension ui, CostExtension cost, LoadoutExtension loadout, VeterancyExtension veterancy,
        string[] types, string[] abilities, string[] upgrades, string[] appliedUpgrades,
        int upgradeCapacity, int slotCapacity, bool canPickup, bool isTeamWpn, bool hasCrew, float femaleChance) {

        // Set properties
        Name = name;
        PBGID = pbgid;
        UI = ui;
        Cost = cost;
        Army = faction;
        IsTeamWeapon = isTeamWpn;
        Types = new(types, isTeamWpn);
        Loadout = loadout;
        Veterancy = veterancy;
        PickupCapacity = slotCapacity;
        CanPickupItems = canPickup;
        Abilities = abilities;
        FemaleSquadChance = femaleChance;
        HasCrew = hasCrew;
        Upgrades = upgrades;
        AppliedUpgrades = appliedUpgrades;
        UpgradeCapacity = upgradeCapacity;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public static SquadCategory GetCategory(TypeList types) {
        if (types.IsAntiTank || types.IsHeavyArtillery || types.Contains("mortar") || types.Contains("hmg")) {
            return SquadCategory.Support;
        } else if (types.Contains("vehicle")) {
            return SquadCategory.Vehicle;
        } else if (types.IsCommandUnit) {
            return SquadCategory.Leader;
        }
        return SquadCategory.Infantry;
    }

}
