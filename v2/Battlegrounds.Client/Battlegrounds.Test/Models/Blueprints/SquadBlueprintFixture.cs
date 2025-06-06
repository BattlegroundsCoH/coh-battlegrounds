using Battlegrounds.Models;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Test.Models.Blueprints;

public static class SquadBlueprintFixture {

    public static readonly SquadBlueprint SBP_TOMMY_UK = new SquadBlueprint("tommy_uk", SquadCategory.Infantry, [
        new UIExtension((LocaleString)"Infantry Section", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(300, 0, 0),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

    public static readonly SquadBlueprint SBP_HALFTRACK_M3_UK = new SquadBlueprint("halftrack_m3_uk", SquadCategory.Support, [
        new UIExtension((LocaleString)"M3 Halftrack", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(260, 0, 15),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

    public static readonly SquadBlueprint SBP_MATILDA_UK = new SquadBlueprint("matilda_uk", SquadCategory.Armour, [
        new UIExtension((LocaleString)"Matilda Infantry Support Vehicle", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(360, 0, 80),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

    public static readonly SquadBlueprint SBP_CRUSADER_UK = new SquadBlueprint("crusader_uk", SquadCategory.Armour, [
        new UIExtension((LocaleString)"Crusader Tank", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(320, 0, 60),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

    public static readonly SquadBlueprint SBP_PANZERGRENADIER_AK = new SquadBlueprint("panzergrenadier_ak", SquadCategory.Infantry, [
        new UIExtension((LocaleString)"Panzergrenadiers", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(300, 0, 0),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

    public static readonly SquadBlueprint SBP_HALFTRACK_250_AK = new SquadBlueprint("halftrack_250_ak", SquadCategory.Support, [
        new UIExtension((LocaleString)"SdKfz 250 Halftrack", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(260, 0, 15),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

    public static readonly SquadBlueprint SBP_PANZER_III_AK = new SquadBlueprint("panzer_iii_ak", SquadCategory.Armour, [
        new UIExtension((LocaleString)"Panzer III", LocaleString.Empty, LocaleString.Empty, "", "", ""),
        new CostExtension(360, 0, 80),
        new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
            new VeterancyExtension.VeterancyRank(4200, "Vet3")
            ])
    ]);

}
