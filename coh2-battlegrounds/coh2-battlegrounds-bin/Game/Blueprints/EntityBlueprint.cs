using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;

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

    /// <summary>
    /// 
    /// </summary>
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
    public string[]? Types { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public int UpgradeCapacity { get; }

    /// <summary>
    /// 
    /// </summary>
    public float Health { get; }

    ///
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
