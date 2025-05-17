using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public enum SquadCategory : byte {
    Infantry,
    Support,
    Armour
}

public sealed class SquadBlueprint(string id, SquadCategory category, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {
    
    public CostExtension Cost => GetExtension<CostExtension>();

    public UIExtension UI => GetExtension<UIExtension>();

    public LoadoutExtension Loadout => GetExtension<LoadoutExtension>();

    public VeterancyExtension Veterancy => GetExtension<VeterancyExtension>();

    public SquadCategory Cateogry => category;

}
