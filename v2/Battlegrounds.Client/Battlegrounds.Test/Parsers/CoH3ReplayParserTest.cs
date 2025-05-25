using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;
using Battlegrounds.Test.Models.Replays;

namespace Battlegrounds.Test.Parsers;

[TestFixture]
public class CoH3ReplayParserTest {

    private CoH3ReplayParser _parser;

    [SetUp]
    public void Setup() {
        _parser = new CoH3ReplayParser();
    }

    [Test]
    public void CanParseSampleCoH3ReplayWithNoBgEvents() {
        string replayLocation = ReplayFixture.TEMP_10_05_2025__20_55_FILE;

        var replay = _parser.ParseReplayFile(replayLocation);
        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        Assert.Multiple(() => {
            Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId), "Replay Game ID should match CoH3.");
            Assert.That(replay.Players, Has.Count.EqualTo(2), "Replay should have 2 players.");
            Assert.That(replay.Duration, Is.GreaterThan(TimeSpan.Zero), "Replay duration should be greater than zero.");
            Assert.That(replay.Events, Is.Empty, "Replay should have no events.");
        });

    }

    [Test]
    public void CanParseSampleCoH3ReplayWithBroadcastMessages() {
        string replayLocation = ReplayFixture.TEMP_10_05_2025__21_16_FILE;

        var replay = _parser.ParseReplayFile(replayLocation);
        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        Assert.Multiple(() => {
            Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId), "Replay Game ID should match CoH3.");
            Assert.That(replay.Players, Has.Count.EqualTo(2), "Replay should have 2 players.");
            Assert.That(replay.Duration, Is.GreaterThan(TimeSpan.Zero), "Replay duration should be greater than zero.");
            Assert.That(replay.Events, Is.Empty, "Replay should have no events.");
        });

    }

    [Test]
    public void CanParseSampleCoH3ReplayWithBgBroadcastEvents() {
        string replayLocation = ReplayFixture.TEMP_23_05_2025__23_51_FILE;
        var replay = _parser.ParseReplayFile(replayLocation);
        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        Assert.Multiple(() => {
            Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId), "Replay Game ID should match CoH3.");
            Assert.That(replay.Players, Has.Count.EqualTo(2), "Replay should have 2 players.");
            Assert.That(replay.Duration, Is.GreaterThan(TimeSpan.Zero), "Replay duration should be greater than zero.");
            Assert.That(replay.Events, Has.Count.GreaterThan(0), "Replay should have events.");
            Assert.That(replay.Events.OfType<MatchStartReplayEvent>().SingleOrDefault(), Is.Not.Null, "Replay should contain a MatchStartReplayEvent.");
            Assert.That(replay.Events.OfType<SquadDeployedEvent>().Count(), Is.EqualTo(6), "Replay should contain SquadDeployedEvent(s).");
        });

    }

}
