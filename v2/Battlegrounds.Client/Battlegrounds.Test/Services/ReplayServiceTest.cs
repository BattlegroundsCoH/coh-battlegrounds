using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Services;

namespace Battlegrounds.Test.Services;

[TestFixture]
public class ReplayServiceTest {

    private ReplayService _replayService;

    [SetUp]
    public void Setup() {
        _replayService = new ReplayService();
    }

    [Test]
    public async Task CanParseSampleCoH3ReplayWithNoBgEvents() {
        string replayLocation = GetReplayLocation("temp_10-05-2025__20_55.rec");
        Assert.That(File.Exists(replayLocation), Is.True, $"Replay file not found: {replayLocation}");

        var replay = await _replayService.AnalyseReplay<CoH3>(replayLocation);
        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        Assert.Multiple(() => {
            Assert.That(replay.Failed, Is.False);
            Assert.That(replay.Replay, Is.Not.Null, "Replay data should not be null.");
            Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId), "Game ID should match CoH3.");
        });
        Assert.Multiple(() => {
            Assert.That(replay.Replay.GameId, Is.EqualTo(CoH3.GameId), "Replay Game ID should match CoH3.");
            Assert.That(replay.Replay.Players, Has.Count.EqualTo(2), "Replay should have 2 players.");
            Assert.That(replay.Replay.Duration, Is.GreaterThan(TimeSpan.Zero), "Replay duration should be greater than zero.");
            Assert.That(replay.Replay.Events, Is.Empty, "Replay should have no events.");
        });

    }

    [Test]
    public async Task CanParseSampleCoH3ReplayWithBroadcastMessages() {
        string replayLocation = GetReplayLocation("temp_10-05-2025__21_16.rec"); // Bad replay file since it lacks actual bg_events, but it does have broadcast messages (that are ignored)
        Assert.That(File.Exists(replayLocation), Is.True, $"Replay file not found: {replayLocation}");

        var replay = await _replayService.AnalyseReplay<CoH3>(replayLocation);
        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        Assert.Multiple(() => {
            Assert.That(replay.Failed, Is.False);
            Assert.That(replay.Replay, Is.Not.Null, "Replay data should not be null.");
            Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId), "Game ID should match CoH3.");
        });
        Assert.Multiple(() => {
            Assert.That(replay.Replay.GameId, Is.EqualTo(CoH3.GameId), "Replay Game ID should match CoH3.");
            Assert.That(replay.Replay.Players, Has.Count.EqualTo(2), "Replay should have 2 players.");
            Assert.That(replay.Replay.Duration, Is.GreaterThan(TimeSpan.Zero), "Replay duration should be greater than zero.");
            Assert.That(replay.Replay.Events, Is.Empty, "Replay should have no events.");
        });

    }

    [Test]
    public async Task CanParseSampleCoH3ReplayWithBgBroadcastEvents() {
        string replayLocation = GetReplayLocation("temp_23-05-2025__23_51.rec");
        Assert.That(File.Exists(replayLocation), Is.True, $"Replay file not found: {replayLocation}");

        var replay = await _replayService.AnalyseReplay<CoH3>(replayLocation);
        Assert.That(replay, Is.Not.Null, "Replay should not be null.");
        Assert.Multiple(() => {
            Assert.That(replay.Failed, Is.False);
            Assert.That(replay.Replay, Is.Not.Null, "Replay data should not be null.");
            Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId), "Game ID should match CoH3.");
        });
        Assert.Multiple(() => {
            Assert.That(replay.Replay.GameId, Is.EqualTo(CoH3.GameId), "Replay Game ID should match CoH3.");
            Assert.That(replay.Replay.Players, Has.Count.EqualTo(2), "Replay should have 2 players.");
            Assert.That(replay.Replay.Duration, Is.GreaterThan(TimeSpan.Zero), "Replay duration should be greater than zero.");
            Assert.That(replay.Replay.Events, Has.Count.GreaterThan(0), "Replay should have events.");
            Assert.That(replay.Replay.Events.OfType<MatchStartReplayEvent>().SingleOrDefault(), Is.Not.Null, "Replay should contain a MatchStartReplayEvent.");
            Assert.That(replay.Replay.Events.OfType<SquadDeployedEvent>().Count(), Is.EqualTo(6), "Replay should contain SquadDeployedEvent(s).");
        });

    }

    private static string GetReplayLocation(string replayName) {
        return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Playbacks", replayName);
    }

}
