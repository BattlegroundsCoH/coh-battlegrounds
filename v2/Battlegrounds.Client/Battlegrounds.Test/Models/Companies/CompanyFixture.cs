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
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK
            },
            new Squad {
                Id = 2,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK
            },
            new Squad {
                Id = 3,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK
            },
            new Squad {
                Id = 4,
                Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
                Experience = 2750.0f
            }
        ]
    };

}
