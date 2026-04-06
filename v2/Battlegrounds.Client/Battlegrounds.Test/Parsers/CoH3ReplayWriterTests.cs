using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;
using Battlegrounds.Test.Models.Replays;

namespace Battlegrounds.Test.Parsers;

[TestFixture]
public class CoH3ReplayWriterTests {

    private CoH3ReplayWriter _writer;
    private CoH3ReplayParser _parser;

    [SetUp]
    public void Setup() {
        _writer = new CoH3ReplayWriter();
        _parser = new CoH3ReplayParser();
    }

    [Test]
    public void CanRoundTripDummyReplayWithoutEvents() {
        var players = new[] {
            ReplayPlayerFixture.CODIEX,
            new ReplayPlayer(1001, 1, "Opponent", 1234, 76561198000000001, "german", string.Empty)
        };

        var replay = ParseDummyReplay(players, []);

        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        using (Assert.EnterMultipleScope()) {
            Assert.That(replay.Players, Is.EqualTo(players), "Players should round-trip through the dummy replay.");
            Assert.That(replay.Events, Is.Empty, "Replay should not contain any events.");
            Assert.That(replay.Duration, Is.EqualTo(TimeSpan.Zero), "Replay duration should be zero when there are no ticks.");
        }
    }

    [Test]
    public void CanRoundTripDummyReplayWithSupportedEvents() {
        var players = new[] {
            new ReplayPlayer(1000, 0, "CoDiEx", 376, 76561198000000000, "british_africa", string.Empty),
            new ReplayPlayer(1001, 1, "Opponent", 512, 76561198000000001, "german", string.Empty)
        };

        var expectedEvents = new ReplayEvent[] {
            new MatchStartReplayEvent(
                TimeSpan.Zero,
                "match-1",
                "1.0.0",
                "pachino_2p",
                [
                    new MatchStartReplayEvent.PlayerData(players[0].PlayerId, players[0].PlayerName, "company-a", 7),
                    new MatchStartReplayEvent.PlayerData(players[1].PlayerId, players[1].PlayerName, "company-b", 8)
                ]),
            new SquadDeployedEvent(TimeSpan.FromSeconds(CoH3ReplayParser.COH3_TICK_RATE), players[0], 2),
            new SquadKilledEvent(TimeSpan.FromSeconds(CoH3ReplayParser.COH3_TICK_RATE * 2), players[1], 3),
            new SquadRecalledEvent(TimeSpan.FromSeconds(CoH3ReplayParser.COH3_TICK_RATE * 3), players[0], 4, 473.91f, 5, 1, 2),
            new SquadWeaponPickupEvent(TimeSpan.FromSeconds(CoH3ReplayParser.COH3_TICK_RATE * 4), players[1], 6, "weapon_breda", true),
            new MatchOverReplayEvent(
                TimeSpan.FromSeconds(CoH3ReplayParser.COH3_TICK_RATE * 5),
                [players[0].PlayerId],
                [players[1].PlayerId],
                [
                    new MatchOverReplayEvent.PlayerStatistics(players[0].PlayerId, players[0].TeamId, players[0].PlayerName, 7, 10, 2),
                    new MatchOverReplayEvent.PlayerStatistics(players[1].PlayerId, players[1].TeamId, players[1].PlayerName, 8, 2, 10)
                ])
        };

        var replay = ParseDummyReplay(players, expectedEvents);

        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        using (Assert.EnterMultipleScope()) {
            Assert.That(replay.Players, Is.EqualTo(players), "Players should round-trip through the dummy replay.");
            Assert.That(replay.Events, Has.Count.EqualTo(expectedEvents.Length), "Supported replay events should round-trip through the dummy replay.");
            Assert.That(replay.Duration, Is.EqualTo(TimeSpan.FromSeconds(CoH3ReplayParser.COH3_TICK_RATE * expectedEvents.Length)), "Replay duration should reflect the written ticks.");
        }

        Assert.That(replay.Events[0], Is.EqualTo(expectedEvents[0]).Using<MatchStartReplayEvent>((actual, expected) =>
            actual.MatchId == expected.MatchId
            && actual.ModVersion == expected.ModVersion
            && actual.Scenario == expected.Scenario
            && actual.Timestamp == expected.Timestamp
            && actual.Players.SequenceEqual(expected.Players) ? 0 : 1), "Match start event should round-trip.");
        Assert.That(replay.Events[1], Is.EqualTo(expectedEvents[1]), "Squad deployed event should round-trip.");
        Assert.That(replay.Events[2], Is.EqualTo(expectedEvents[2]), "Squad killed event should round-trip.");
        Assert.That(replay.Events[3], Is.EqualTo(expectedEvents[3]), "Squad recalled event should round-trip.");
        Assert.That(replay.Events[4], Is.EqualTo(expectedEvents[4]), "Weapon pickup event should round-trip.");
        Assert.That(replay.Events[5], Is.EqualTo(expectedEvents[5]).Using<MatchOverReplayEvent>((actual, expected) =>
            actual.Timestamp == expected.Timestamp
            && actual.Winners.SequenceEqual(expected.Winners)
            && actual.Losers.SequenceEqual(expected.Losers)
            && actual.PlayerStats.SequenceEqual(expected.PlayerStats) ? 0 : 1), "Match over event should round-trip.");
    }

    private Replay ParseDummyReplay(ReplayPlayer[] players, ReplayEvent[] events) {
        string replayPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.rec");

        try {
            byte[] bytes = _writer.WriteDummyReplay(players, events);
            File.WriteAllBytes(replayPath, bytes);
            return _parser.ParseReplayFile(replayPath);
        } finally {
            if (File.Exists(replayPath)) {
                File.Delete(replayPath);
            }
        }
    }

}
