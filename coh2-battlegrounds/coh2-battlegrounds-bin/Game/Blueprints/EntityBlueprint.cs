using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Representation of a <see cref="Blueprint"/> with <see cref="Entity"/> specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
/// </summary>
public sealed class EntityBlueprint : Blueprint, IUIBlueprint {

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    public override BlueprintType BlueprintType => BlueprintType.EBP;

    public override string Name { get; }

    public CostExtension Cost { get; }

    public UIExtension UI { get; }

    public DriverExtension Drivers { get; }

    public Faction? Faction { get; }

    public string[] Abilities { get; }

    public string[] Hardpoints { get; }

    public string[] Upgrades { get; }

    public string[] AppliedUpgrades { get; }

    public int UpgradeCapacity { get; }

    public float Health { get; }

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
