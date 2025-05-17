using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public enum SquadCateogry : byte {
    Infantry,
    Support,
    Armour
}

public sealed class SquadBlueprint(string id, SquadCateogry cateogry, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {
    
    public CostExtension Cost => GetExtension<CostExtension>();

    public UIExtension UI => GetExtension<UIExtension>();

    public LoadoutExtension Loadout => GetExtension<LoadoutExtension>();

    public VeterancyExtension Veterancy => GetExtension<VeterancyExtension>();

    public SquadCateogry Cateogry => cateogry;

}
