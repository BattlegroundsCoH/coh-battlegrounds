using System.Text;

using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Scripts;
using Battlegrounds.Core.Matches;
using Battlegrounds.Core.Test.Companies;
using Battlegrounds.Core.Test.Games.Gamemodes;
using Battlegrounds.Core.Test.Games.Scenarios;

namespace Battlegrounds.Core.Test.Matches;

public class MatchDataBuilderTest {

    private MatchDataBuilder builder;

    [SetUp]
    public void SetUp() {
        builder = new MatchDataBuilder(CoreTest.Services.GetRequiredService<ILogger<MatchDataBuilder>>());
    }

    [Test]
    public void MatchDataBuilder_WillConstructBasicScarFile() {

        using MemoryStream stream = new MemoryStream();
        using (ScarScriptWriter writer = new ScarScriptWriter(stream)) {

            MatchData data = new MatchData(GamemodeFixture.CoH3_VictoryPoints, ScenarioFixture.desert_village_2p_mkiii,
                [new MatchPlayer(0, 1ul, CompanyFixture.DesertRats, "Player 1", AIDifficulty.AI_HUMAN)],
                [new MatchPlayer(0, 2ul, CompanyFixture.AfrikaKorps, "Player 2", AIDifficulty.AI_HUMAN)],
                new Dictionary<string, string> {

                });

            var success = builder.Build(writer, data);

            Assert.That(success, Is.True);

        }

        Assert.That(stream.Length, Is.GreaterThan(0));

        string content = Encoding.UTF8.GetString(stream.ToArray());

        // Log it
        Console.WriteLine(content);

        Assert.Multiple(() => {
            Assert.That(content, Contains.Substring("\"Player 1\""));
            Assert.That(content, Contains.Substring("\"Player 2\""));
            Assert.That(content, Contains.Substring(CompanyFixture.DesertRats.Name));
            Assert.That(content, Contains.Substring(CompanyFixture.AfrikaKorps.Name));
            Assert.That(content, Contains.Substring(CompanyFixture.DesertRats.Id.ToString()));
            Assert.That(content, Contains.Substring(CompanyFixture.AfrikaKorps.Id.ToString()));
            // TODO: More checks
        });

    }

}
