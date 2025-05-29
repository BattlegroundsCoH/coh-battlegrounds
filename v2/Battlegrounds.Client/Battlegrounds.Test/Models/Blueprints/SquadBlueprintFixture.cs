using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Test.Models.Blueprints;

public static class SquadBlueprintFixture {

    public static readonly SquadBlueprint SBP_TOMMY_UK = new SquadBlueprint("tommy_uk", SquadCategory.Infantry, [
        new UIExtension("Infantry Section", "", "", "", "", ""),
        new CostExtension(300.0f, 0.0f, 0.0f),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

}
