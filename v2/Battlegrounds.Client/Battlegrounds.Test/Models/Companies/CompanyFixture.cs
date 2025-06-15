using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Test.Models.Blueprints;

namespace Battlegrounds.Test.Models.Companies;

public static class CompanyFixture {

    public static readonly Company DESERT_RATS = new Company {
        Id = "desert_rats",
        Name = "Desert Rats",
        Faction = "british_africa",
        GameId = CoH3.GameId,
        Squads = [
            new Squad {
                Id = 1,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
            },
            new Squad {
                Id = 2,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
            },
            new Squad {
                Id = 3,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
            },
            new Squad {
                Id = 4,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
                Experience = 2750.0f,
            },
            new Squad {
                Id = 5,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
            },
            new Squad {
                Id = 17,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
                Experience = 5400.0f,
                Transport = new Squad.TransportSquad(SquadBlueprintFixture.SBP_HALFTRACK_M3_UK, DropOffOnly: true),
            },
            new Squad {
                Id = 19,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
                Experience = 5400.0f,
                Upgrades = [UpgradeBlueprintFixture.UPG_LMG_BREN],
            },
            new Squad {
                Id = 28,
                Blueprint = SquadBlueprintFixture.SBP_MATILDA_UK,
            },
        ]
    };

    public static readonly Company AFRIKA_KORPS = new Company {
        Id = "afrika_korps",
        Name = "Afrika Korps",
        Faction = "afrika_korps",
        GameId = CoH3.GameId,
        Squads = [
            new Squad {
                Id = 1,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
            },
            new Squad {
                Id = 2,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
            },
            new Squad {
                Id = 3,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
            },
            new Squad {
                Id = 4,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
                Experience = 2750.0f,
            },
            new Squad {
                Id = 5,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
            },
            new Squad {
                Id = 17,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
                Experience = 5400.0f,
                Transport = new Squad.TransportSquad(SquadBlueprintFixture.SBP_HALFTRACK_250_AK, DropOffOnly: true),
            },
            new Squad {
                Id = 19,
                Blueprint = SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
                Experience = 5400.0f,
                Upgrades = [UpgradeBlueprintFixture.UPG_LMG_PANZERGRENADIER_AK],
            },
            new Squad {
                Id = 28,
                Blueprint = SquadBlueprintFixture.SBP_PANZER_III_AK,
            },
        ]
    };

}
