using System;

using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Meta.Annotations;

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
    toggle,

    /// <summary>
    /// The ability targets self (CoH3 Only)
    /// </summary>
    [GameSpecific(GameCase.CompanyOfHeroes3)]
    self_targeted

}

/// <summary>
/// Representation of a <see cref="Blueprint"/> with ability specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inherited.
/// </summary>
public sealed class AbilityBlueprint : Blueprint, IUIBlueprint {

    /// <summary>
    /// Invalid ability blueprint instance with no data associated with it.
    /// </summary>
    public static readonly AbilityBlueprint Invalid = new("abp_invalid", new(), null, new(), new(), Array.Empty<RequirementExtension>(), AbilityActivation.none);

    /// <inheritdoc/>
    public override BlueprintUID PBGID { get; }

    /// <inheritdoc/>
    public override BlueprintType BlueprintType => BlueprintType.ABP;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <summary>
    /// Get the cost extension of the ability.
    /// </summary>
    public CostExtension Cost { get; }

    /// <inheritdoc/>
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
    /// Get if the ability has a facing phase (ie. a phase where a direction is set).
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
