using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;
using Battlegrounds.Test.Models.Replays;

namespace Battlegrounds.Test.Parsers;

[TestFixture]
public class ReplayEventParserTests {

    [Test]
    public void CanParseSquadKilledEvent() {
        string encodedStr = "bg_match_event({<player=1000><type=squad_killed><companyId=2>})";
        var players = new[] { ReplayPlayerFixture.CODIEX };
        var timestamp = TimeSpan.FromSeconds(10);
        var replayEvent = ReplayEventParser.ParseEvent(encodedStr, players, timestamp);
        Assert.That(replayEvent, Is.Not.Null, "Replay event should not be null.");
        Assert.That(replayEvent, Is.InstanceOf<SquadKilledEvent>(), "Replay event should be of type SquadKilledEvent.");
        var squadKilledEvent = replayEvent as SquadKilledEvent;
        Assert.That(squadKilledEvent, Is.Not.Null, "SquadKilledEvent should not be null.");
        Assert.Multiple(() => {
            Assert.That(squadKilledEvent.Player, Is.EqualTo(players[0]), "Player object should match.");
            Assert.That(squadKilledEvent.SquadCompanyId, Is.EqualTo(2), "Company ID should match.");
        });
    }

    [Test]
    public void CanParseSquadDeployedEvent() {
        string encodedStr = "bg_match_event({<player=1000><type=squad_deployed><companyId=2>})";
        var players = new[] { ReplayPlayerFixture.CODIEX };
        var timestamp = TimeSpan.FromSeconds(10);
        var replayEvent = ReplayEventParser.ParseEvent(encodedStr, players, timestamp);
        Assert.That(replayEvent, Is.Not.Null, "Replay event should not be null.");
        Assert.That(replayEvent, Is.InstanceOf<SquadDeployedEvent>(), "Replay event should be of type SquadDeployedEvent.");
        var squadDeployedEvent = replayEvent as SquadDeployedEvent;
        Assert.That(squadDeployedEvent, Is.Not.Null, "SquadDeployedEvent should not be null.");
        Assert.Multiple(() => {
            Assert.That(squadDeployedEvent.Player, Is.EqualTo(players[0]), "Player object should match.");
            Assert.That(squadDeployedEvent.SquadCompanyId, Is.EqualTo(2), "Company ID should match.");
        });
    }

    [Test]
    public void CanParseSquadWeaponPickupEvent() {
        string encodedStr = "bg_match_event({<player=1000><type=item_pickup><companyId=2><ebp=lmg_bren>})";
        var players = new[] { ReplayPlayerFixture.CODIEX };
        var timestamp = TimeSpan.FromSeconds(10);
        var replayEvent = ReplayEventParser.ParseEvent(encodedStr, players, timestamp);
        Assert.That(replayEvent, Is.Not.Null, "Replay event should not be null.");
        Assert.That(replayEvent, Is.InstanceOf<SquadWeaponPickupEvent>(), "Replay event should be of type SquadWeaponPickupEvent.");
        var squadWeaponPickupEvent = replayEvent as SquadWeaponPickupEvent;
        Assert.That(squadWeaponPickupEvent, Is.Not.Null, "SquadWeaponPickupEvent should not be null.");
        Assert.Multiple(() => {
            Assert.That(squadWeaponPickupEvent.Player, Is.EqualTo(players[0]), "Player object should match.");
            Assert.That(squadWeaponPickupEvent.SquadCompanyId, Is.EqualTo(2), "Company ID should match.");
            Assert.That(squadWeaponPickupEvent.WeaponName, Is.EqualTo("lmg_bren"), "Weapon name should match.");
            Assert.That(squadWeaponPickupEvent.IsEntityBlueprint, Is.True, "IsEntityBlueprint should be true.");
        });
    }

    [Test]
    public void CanParseMatchStartEvent() {
        string encodedStr = "bg_match_event({<type=match_data><match_id=0><mod_version=1.0><scenario=pachino_2p><playerdata={<1000={<mod_id=0><name=CoDiEx><company=df6100e1-30cd-4338-ac81-8d54d60f6c29>}>}>})";
        var players = new[] { ReplayPlayerFixture.CODIEX };
        var timestamp = TimeSpan.FromSeconds(10);
        var replayEvent = ReplayEventParser.ParseEvent(encodedStr, players, timestamp);
        Assert.That(replayEvent, Is.Not.Null, "Replay event should not be null.");
        Assert.That(replayEvent, Is.InstanceOf<MatchStartReplayEvent>(), "Replay event should be of type MatchStartReplayEvent.");
        var matchStartEvent = replayEvent as MatchStartReplayEvent;
        Assert.That(matchStartEvent, Is.Not.Null, "MatchStartReplayEvent should not be null.");
        Assert.Multiple(() => {
            Assert.That(matchStartEvent.MatchId, Is.EqualTo("0"), "Match ID should match.");
            Assert.That(matchStartEvent.ModVersion, Is.EqualTo("1.0"), "Mod version should match.");
            Assert.That(matchStartEvent.Timestamp, Is.EqualTo(timestamp), "Timestamp should match.");
            Assert.That(matchStartEvent.Player, Is.Null, "Player should be null for MatchStartReplayEvent.");
            Assert.That(matchStartEvent.Scenario, Is.EqualTo("pachino_2p"), "Scenario should match.");
            Assert.That(matchStartEvent.Players, Has.Count.EqualTo(1), "There should be one player in the MatchStartReplayEvent.");
            var playerData = matchStartEvent.Players[0];
            Assert.That(playerData.PlayerId, Is.EqualTo(1000), "Player ID should match.");
            Assert.That(playerData.Name, Is.EqualTo("CoDiEx"), "Player name should match.");
            Assert.That(playerData.CompanyId, Is.EqualTo("df6100e1-30cd-4338-ac81-8d54d60f6c29"), "Company ID should match.");
            Assert.That(playerData.ModId, Is.EqualTo(0), "Mod ID should match.");
        });
    }

}
