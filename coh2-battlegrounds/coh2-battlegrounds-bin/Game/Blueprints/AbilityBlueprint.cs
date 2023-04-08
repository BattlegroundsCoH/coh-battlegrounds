using System;

using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Enum representing the method in which an ability is activated.
/// </summary>
public enum AbilityActivation {

    /// <summary>
    /// The ability has no activation method
    /// </summary>
    none,

    /// <summary>
    /// The ability is always on.
    /// </summary>
    always_on,

    /// <summary>
    /// The ability requires a target.
    /// </summary>
    targeted,

    /// <summary>
    /// The ability is timed.
    /// </summary>
    timed,

    /// <summary>
    /// The ability is toggled.
    /// </summary>
    toggle

}

/// <summary>
/// Representation of a <see cref="Blueprint"/> with ability specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inherited.
/// </summary>
public sealed class AbilityBlueprint : Blueprint, IUIBlueprint {

    /// <summary>
    /// Invalid ability blueprint instance with no data associated with it.
    /// </summary>
    public static readonly AbilityBlueprint Invalid = new("abp_invalid", new(), null, new(), new(), Array.Empty<RequirementExtension>(), AbilityActivation.none);

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    /// <summary>
    /// 
    /// </summary>
    public override BlueprintType BlueprintType => BlueprintType.ABP;

    /// <summary>
    /// 
    /// </summary>
    public override string Name { get; }

    /// <summary>
    /// Get the cost extension of the ability.
    /// </summary>
    public CostExtension Cost { get; }

    /// <summary>
    /// Get the UI extension of the ability.
    /// </summary>
    public UIExtension UI { get; }

    /// <summary>
    /// Get the faction associated with the ability.
    /// </summary>
    public Faction? Faction { get; }

    /// <summary>
    /// Get the requirements required for using this ability.
    /// </summary>
    public RequirementExtension[] Requirements { get; }

    /// <summary>
    /// Get the activiation method of the ability.
    /// </summary>
    public AbilityActivation Activation { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool HasFacingPhase { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="blueprintUID"></param>
    /// <param name="faction"></param>
    /// <param name="cost"></param>
    /// <param name="ui"></param>
    /// <param name="requirements"></param>
    /// <param name="abilityActivation"></param>
    public AbilityBlueprint(string name, BlueprintUID blueprintUID, Faction? faction, CostExtension cost, UIExtension ui,
        RequirementExtension[] requirements, AbilityActivation abilityActivation) {
        Cost = cost;
        UI = ui;
        Faction = faction;
        Name = name;
        PBGID = blueprintUID;
        Requirements = requirements;
        Activation = abilityActivation;
    }

}
