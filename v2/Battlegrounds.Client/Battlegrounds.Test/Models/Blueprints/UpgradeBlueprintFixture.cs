using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Test.Models.Blueprints;

public static class UpgradeBlueprintFixture {

    public static readonly UpgradeBlueprint UPG_LMG_BREN = new UpgradeBlueprint("lmg_bren_tommy_uk", [
        new CostExtension(0.0f, 70.0f, 0.0f)
        ]);

}
