using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Meta.Annotations;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Representation of a <see cref="Blueprint"/> with <see cref="Entity"/> specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
/// </summary>
public sealed class EntityBlueprint : Blueprint, IUIBlueprint {

    /// <inheritdoc/>
    public override BlueprintUID PBGID { get; }

    /// <inheritdoc/>
    public override BlueprintType BlueprintType => BlueprintType.EBP;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <summary>
    /// 
    /// </summary>
    public CostExtension Cost { get; }

    /// <inheritdoc/>
    public UIExtension UI { get; }

    /// <summary>
    /// 
    /// </summary>
    public DriverExtension Drivers { get; }

    /// <summary>
    /// 
    /// </summary>
    public Faction? Faction { get; }

    /// <summary>
    /// 
    /// </summary>
    public string[] Abilities { get; }

    /// <summary>
    /// 
    /// </summary>
    public string[] Hardpoints { get; }

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
    [GameSpecific(GameCase.CompanyOfHeroes3)]
    public string[]? Types { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public int UpgradeCapacity { get; }

    /// <summary>
    /// 
    /// </summary>
    public float Health { get; }

    /// <summary>
    /// 
    /// </summary>
    [GameSpecific(GameCase.CompanyOfHeroes3)]
    public bool IsInventoryItem { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [GameSpecific(GameCase.CompanyOfHeroes3)]
    public int InventoryRequiredCapacity { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [GameSpecific(GameCase.CompanyOfHeroes3)]
    public float InventoryDropOnDeathChance { get; init; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pbgid"></param>
    /// <param name="faction"></param>
    /// <param name="cost"></param>
    /// <param name="ui"></param>
    /// <param name="driverExtension"></param>
    /// <param name="abilities"></param>
    /// <param name="upgrades"></param>
    /// <param name="appliedUpgrades"></param>
    /// <param name="upgradeMax"></param>
    /// <param name="hardpoints"></param>
    /// <param name="health"></param>
    public EntityBlueprint(string name, BlueprintUID pbgid, Faction? faction,
        CostExtension cost, UIExtension ui, DriverExtension driverExtension,
        string[] abilities, string[] upgrades, string[] appliedUpgrades, int upgradeMax, string[] hardpoints, float health) {
        Name = name;
        PBGID = pbgid;
        UI = ui;
        Cost = cost;
        Faction = faction;
        Abilities = abilities;
        Hardpoints = hardpoints;
        Health = health;
        Drivers = driverExtension;
        UpgradeCapacity = upgradeMax;
        Upgrades = upgrades;
        AppliedUpgrades = appliedUpgrades;
    }

}
