using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public sealed class UpgradeBlueprint(string id, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {

    public UIExtension UI => GetExtension<UIExtension>();

    public CostExtension Cost => GetExtension<CostExtension>();

}
