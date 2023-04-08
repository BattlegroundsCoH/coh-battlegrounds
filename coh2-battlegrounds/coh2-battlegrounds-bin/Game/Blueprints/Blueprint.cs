using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Represents a <see cref="Blueprint"/> for the behaviour of instances within Company of Heroes.
/// </summary>
public abstract class Blueprint {

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public abstract BlueprintUID PBGID { get; }

    /// <summary>
    /// The unique PropertyBagGroupID assigned to this blueprint at load-time.
    /// </summary>
    public ushort ModPBGID { get; set; }

    /// <summary>
    /// The name of the <see cref="Blueprint"/> file in the game files (See the instances folder in the mod tools).
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The type of <see cref="Blueprint"/>.
    /// </summary>
    public abstract BlueprintType BlueprintType { get; }

    /// <summary>
    /// The game this blueprint is for.
    /// </summary>
    public GameCase Game { get; init; }

    /// <inheritdoc/>
    public override string ToString() => $"{BlueprintType}:{Name}";

    /// <inheritdoc/>
    public override bool Equals(object? obj) {
        if (obj is Blueprint bp && this != null) {
            return bp.BlueprintType == BlueprintType && bp.PBGID == PBGID;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Get the scar name of the blueprint.
    /// </summary>
    /// <returns>The name of the blueprint for scar context.</returns>
    public string GetScarName() {
        if (PBGID.Mod == ModGuid.BaseGame) {
            return Name;
        } else {
            return $"{PBGID.Mod.GUID}:{Name}";
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode() => PBGID.GetHashCode();

    /// <summary>
    /// Get the <see cref="Blueprints.BlueprintType"/> from the given type parameter <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Concrete <see cref="Blueprint"/> type.</typeparam>
    /// <returns>The <see cref="Blueprints.BlueprintType"/> associated with <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException"/>
    public static BlueprintType BlueprintTypeFromType<T>() where T : Blueprint {
        if (typeof(T) == typeof(SquadBlueprint)) {
            return BlueprintType.SBP;
        } else if (typeof(T) == typeof(EntityBlueprint)) {
            return BlueprintType.EBP;
        } else if (typeof(T) == typeof(AbilityBlueprint)) {
            return BlueprintType.ABP;
        } else if (typeof(T) == typeof(SlotItemBlueprint)) {
            return BlueprintType.IBP;
        } else if (typeof(T) == typeof(CriticalBlueprint)) {
            return BlueprintType.CBP;
        } else if (typeof(T) == typeof(UpgradeBlueprint)) {
            return BlueprintType.UBP;
        } else if (typeof(T) == typeof(WeaponBlueprint)) {
            return BlueprintType.WBP;
        } else {
            throw new ArgumentException("Invalid type argument");
        }
    }

}
