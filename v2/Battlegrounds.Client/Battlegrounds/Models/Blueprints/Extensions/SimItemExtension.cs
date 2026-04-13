namespace Battlegrounds.Models.Blueprints.Extensions;

/// <summary>
/// Represents additional configuration for a simulated item, including inventory space, drop chance, and associated
/// weapon blueprint.
/// </summary>
/// <param name="ItemInventorySpace">The number of inventory slots occupied by the item. Must be zero or greater.</param>
/// <param name="DropChance">The probability that the item will be dropped on squad death, expressed as a value between 0.0 and 1.0.</param>
/// <param name="WeaponBlueprint">The identifier of the weapon blueprint associated with the item.</param>
public sealed record SimItemExtension(int ItemInventorySpace, float DropChance, string? WeaponBlueprint) 
    : BlueprintExtension(nameof(SimItemExtension));
