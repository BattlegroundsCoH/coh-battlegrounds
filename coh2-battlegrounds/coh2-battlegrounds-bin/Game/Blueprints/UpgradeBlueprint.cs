using System.Collections.Generic;

using Battlegrounds.Game.Blueprints.Extensions;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Representation of a <see cref="Blueprint"/> with upgrade specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
/// </summary>
public class UpgradeBlueprint : Blueprint, IUIBlueprint {

    public enum OwnerType {
        None,
        Self,
        Player,
        EntityInSquad
    }

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    public override BlueprintType BlueprintType => BlueprintType.UBP;

    public override string Name { get; }

    /// <summary>
    /// Get the <see cref="OwnerType"/> for the upgrade.
    /// </summary>
    public OwnerType OwnershipType { get; }

    /// <inheritdoc/>
    public UIExtension UI { get; }

    /// <summary>
    /// Get the cost extension of the ability.
    /// </summary>
    public CostExtension Cost { get; }

    /// <summary>
    /// The names of the granted <see cref="SlotItemBlueprint"/> by the <see cref="UpgradeBlueprint"/>.
    /// </summary>
    public HashSet<string> SlotItems { get; set; }

    /// <summary>
    /// Get the requirements for getting the upgrade.
    /// </summary>
    public RequirementExtension[] Requirements { get; }

    /// <summary>
    /// New <see cref="UpgradeBlueprint"/> instance. This should only ever be used by the database loader!
    /// </summary>
    public UpgradeBlueprint(string name, BlueprintUID pbgid, OwnerType ownertype, UIExtension ui, CostExtension cost, RequirementExtension[] requirements, string[] slotItems) : base() {
        Name = name;
        PBGID = pbgid;
        OwnershipType = ownertype;
        UI = ui;
        Cost = cost;
        SlotItems = new(slotItems);
        Requirements = requirements;
    }

}
