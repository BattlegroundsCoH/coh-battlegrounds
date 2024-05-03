using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Companies.Builders;
using Battlegrounds.Core.Games.Factions;
using Battlegrounds.Core.Test.Companies.Templates;
using Battlegrounds.Core.Test.Games.Blueprints;

namespace Battlegrounds.Core.Test.Companies;

public static class CompanyFixture {

    public static ICompany DesertRats => new CompanyBuilder()
        .WithId(Guid.Parse("df6100e1-30cd-4338-ac81-8d54d60f6c29"))
        .WithName("Desert Rats")
        .WithTemplate(CompanyTemplateFixture.DESERT_RATS)
        .WithFaction(CoH3Faction.British)
        .WithSquad(
            squad => squad.WithBlueprint(BlueprintFixture.SBP_COH3_BRITISH_TOMMY)
                .AddToCompany(4)
        )
        .Build();

}
