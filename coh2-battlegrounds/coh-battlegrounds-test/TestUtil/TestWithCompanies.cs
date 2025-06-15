using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.DataLocal.Generator;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content;

namespace Battlegrounds.Testing.TestUtil;

public abstract class TestWithCompanies : TestWithMockedBattlegroundsContext {

    private IModPackage _modPackage;

    [SetUp]
    public void AwakeCompanySetup() {
        _modPackage = new ModPackage() {
            FactionSettings = new Dictionary<Faction, FactionData>() {
                [Faction.BritishAfrica] = new(Faction.FactionStrBritishAfrica, Array.Empty<FactionData.Driver>(), Array.Empty<FactionAbility>(), Array.Empty<FactionData.UnitAbility>(), Array.Empty<string>(), Array.Empty<string>(), false, false, new FactionData.CompanySettings(), string.Empty, Game.GameCase.CompanyOfHeroes3),
            }
        };
    }

    protected Company GetCompany(string testCompany) => testCompany switch {
        "coh3_british" => InitialCompanyCreator.CreateDefaultCoH3BritishCompany(_modPackage),
        "coh3_afrika_korps" => InitialCompanyCreator.CreateDefaultCoH3BritishCompany(_modPackage),
        _ => throw new NotSupportedException("Specified test company is not supported")
    };

    // TODO: Define companies here

}
