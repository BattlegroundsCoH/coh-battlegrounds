using Battlegrounds.Models;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Test.Models.Blueprints;

public static class EntityBlueprintFixture {

    public static readonly EntityBlueprint EBP_W_PANZERSHREK_PANZERJAGER_AK = new EntityBlueprint("w_panzerschrek_panzerjager_ak", EntityCategory.Weapon, [
        new UIExtension((LocaleString)"Panzershrek", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new SimItemExtension(1, 0.33f, "panzerschrek_panzerjager_ak")
    ]);

}
