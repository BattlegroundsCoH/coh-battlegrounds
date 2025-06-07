using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public enum SquadCategory : byte {
    Infantry,
    Support,
    Armour
}

public sealed class SquadBlueprint(string id, SquadCategory category, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {

    public bool IsInfantry { get; init; } = false; // TODO: Fetch from a type list at some point

    public CostExtension Cost => GetExtension<CostExtension>();

    public UIExtension UI => GetExtension<UIExtension>();

    public LoadoutExtension Loadout => GetExtension<LoadoutExtension>();

    public VeterancyExtension Veterancy => GetExtension<VeterancyExtension>();

    public UpgradesExtension Upgrades => TryGetExtension(out UpgradesExtension? ext) ? ext : UpgradesExtension.Default;

    public SquadCategory Cateogry => category;

}
