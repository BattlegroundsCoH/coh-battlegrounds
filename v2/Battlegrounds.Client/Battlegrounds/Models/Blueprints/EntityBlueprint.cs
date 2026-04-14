using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public enum EntityCategory : byte {
    Soldier,
    Vehicle,
    TeamWeapon,
    Weapon,
    Item
}

public sealed class EntityBlueprint(string id, EntityCategory category, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {

    public EntityCategory Category { get; init; } = category;

    public TeamWeaponExtension TeamWeapon => TryGetExtension(out TeamWeaponExtension? ext) ? ext : TeamWeaponExtension.None;

    public EntityBlueprint() : this(string.Empty, EntityCategory.Soldier, []) {
        // Default constructor for deserialization or empty instances
    }

}
