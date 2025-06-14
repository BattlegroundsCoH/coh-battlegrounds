namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record UpgradesExtension(int Slots, IList<string> Available) 
    : BlueprintExtension(nameof(UpgradesExtension)) {
    public static readonly UpgradesExtension Default = new(0, []); // Default with no upgrade slots and no upgrades.
}
