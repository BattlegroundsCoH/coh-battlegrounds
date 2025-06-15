using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Replays;

using NSubstitute;

namespace Battlegrounds.Test.Models.Replays;

[TestFixture]
public sealed class ReplayAnalysisResultTests {

    [Test]
    public void GetMatchResult_ReturnsUnknown_WhenReplayIsNull() {

        // Arrange
        var result = new ReplayAnalysisResult {
            Replay = null
        };
        var lobby = Substitute.For<ILobby>();

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.That(matchResult, Is.EqualTo(MatchResult.Unknown));

    }

    [Test]
    public void GetMatchResult_ReturnsValidMatchResult_WhenReplayIsValid() {

        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        ReplayPlayer player2 = new ReplayPlayer(1001, 1, "Player 2", 0, 0, "afrika_korps", "ai_default");
        var replay = new Replay {
            Players = [player1, player2],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0),
                    new MatchStartReplayEvent.PlayerData(1001, "Player 2", "company2", 1)
                ]),
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), player1, 100),
                new SquadKilledEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(2)), player1, 100),
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(3)), [1000], [1001], [])
            ],
            GameId = "TestGameId",
        };

        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };

        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false),
            new Participant(1, "p2", "Player 2", false, false)
        });

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.Multiple(() => {
            Assert.That(matchResult.IsValid, Is.True);
            Assert.That(matchResult.GameId, Is.EqualTo("TestGameId")); // Default value
            Assert.That(matchResult.MatchId, Is.EqualTo("TestId")); // Default value
            Assert.That(matchResult.ModVersion, Is.EqualTo("v1.0")); // Default value
            Assert.That(matchResult.BadEvents, Is.Empty); // No bad events in this case
        });

    }

    [Test]
    public void GetMatchResult_ReturnsInvalidMatchResult_WhenReplayHasBadEvents() {

        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        ReplayPlayer player2 = new ReplayPlayer(1001, 1, "Player 2", 0, 0, "afrika_korps", "ai_default");
        var replay = new Replay {
            Players = [player1, player2],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0),
                    new MatchStartReplayEvent.PlayerData(1001, "Player 2", "company2", 1)
                ]),
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), player1, 100),
                new SquadKilledEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(2)), player1, 200), // Bad event: killed squad not deployed
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(3)), [1000], [1001], [])
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false),
            new Participant(1, "p2", "Player 2", false, false)
        });

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.Multiple(() => {
            Assert.That(matchResult.IsValid, Is.False);
            Assert.That(matchResult.GameId, Is.EqualTo("TestGameId")); // Default value
            Assert.That(matchResult.MatchId, Is.EqualTo("TestId")); // Default value
            Assert.That(matchResult.ModVersion, Is.EqualTo("v1.0")); // Default value
            Assert.That(matchResult.BadEvents, Has.Count.EqualTo(1)); // One bad event due to killed squad not deployed
        });
        Assert.Multiple(() => {
            Assert.That(matchResult.BadEvents[0].Event, Is.InstanceOf<SquadKilledEvent>());
            Assert.That(matchResult.BadEvents[0].Reason, Is.EqualTo("Squad 200 killed without being deployed"));
        });

    }

    [Test]
    public void GetMatchResult_ReturnsInvalidMatchResult_WhenReplayHasMultipleBadEvents() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        ReplayPlayer player2 = new ReplayPlayer(1001, 1, "Player 2", 0, 0, "afrika_korps", "ai_default");
        var replay = new Replay {
            Players = [player1, player2],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0),
                    new MatchStartReplayEvent.PlayerData(1001, "Player 2", "company2", 1)
                ]),
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), player1, 100),
                new SquadKilledEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(2)), player1, 200), // Bad event: killed squad not deployed
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(3)), player2, 300),
                new SquadKilledEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(4)), player2, 400), // Bad event: killed squad not deployed
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(5)), [1000], [1001], [])
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false),
            new Participant(1, "p2", "Player 2", false, false)
        });
        // Act
        var matchResult = result.GetMatchResult(lobby);

        Assert.Multiple(() => {
            Assert.That(matchResult.IsValid, Is.False);
            Assert.That(matchResult.GameId, Is.EqualTo("TestGameId")); // Default value
            Assert.That(matchResult.MatchId, Is.EqualTo("TestId")); // Default value
            Assert.That(matchResult.ModVersion, Is.EqualTo("v1.0")); // Default value
            Assert.That(matchResult.BadEvents, Has.Count.EqualTo(2)); // Two bad events due to killed squads not deployed
        });
        Assert.Multiple(() => {
            Assert.That(matchResult.BadEvents[0].Event, Is.InstanceOf<SquadKilledEvent>());
            Assert.That(matchResult.BadEvents[0].Reason, Is.EqualTo("Squad 200 killed without being deployed"));
            Assert.That(matchResult.BadEvents[1].Event, Is.InstanceOf<SquadKilledEvent>());
            Assert.That(matchResult.BadEvents[1].Reason, Is.EqualTo("Squad 400 killed without being deployed"));
        });

    }

    [Test]
    public void GetMatchResult_ReturnsUnknown_WhenReplayAnalysisFailed() {
        // Arrange
        var result = new ReplayAnalysisResult {
            Failed = true
        };
        var lobby = Substitute.For<ILobby>();
        // Act
        var matchResult = result.GetMatchResult(lobby);
        // Assert
        Assert.That(matchResult, Is.EqualTo(MatchResult.Unknown));
    }

    [Test]
    public void GetMatchResult_ReturnsInvalid_WhenReplayIsNull() {
        // Arrange
        var result = new ReplayAnalysisResult {
            Replay = null
        };
        var lobby = Substitute.For<ILobby>();
        // Act
        var matchResult = result.GetMatchResult(lobby);
        // Assert
        Assert.That(matchResult, Is.EqualTo(MatchResult.Unknown));
    }

    [Test]
    public void GetMatchResult_ReturnsInvalid_WhenReplayHasNoEvents() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        ReplayPlayer player2 = new ReplayPlayer(1001, 1, "Player 2", 0, 0, "afrika_korps", "ai_default");
        var replay = new Replay {
            Players = [player1, player2],
            Events = [], // No events
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false),
            new Participant(1, "p2", "Player 2", false, false)
        });
        // Act
        var matchResult = result.GetMatchResult(lobby);
        // Assert
        Assert.That(matchResult.IsValid, Is.False);
    }

    [Test]
    public void GetMatchResult_ReturnsInvalid_WhenReplayHasNoPlayers() {
        // Arrange
        var replay = new Replay {
            Players = [], // No players
            Events = [], // No events
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        // Act
        var matchResult = result.GetMatchResult(lobby);
        // Assert
        Assert.That(matchResult.IsValid, Is.False);
    }

    [Test]
    public void GetMatchResult_ReturnsInvalid_WhenReplayHasNoStartEvent() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        ReplayPlayer player2 = new ReplayPlayer(1001, 1, "Player 2", 0, 0, "afrika_korps", "ai_default");
        var replay = new Replay {
            Players = [player1, player2],
            Events = [
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), player1, 100),
                new SquadKilledEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(2)), player1, 100),
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(3)), [1000], [1001], [])
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false),
            new Participant(1, "p2", "Player 2", false, false)
        });
        // Act
        var matchResult = result.GetMatchResult(lobby);
        // Assert
        Assert.That(matchResult.IsValid, Is.False);
    }

    [Test]
    public void GetMatchResult_ReturnsInvalidMatchResult_WhenReplayHasNoMatchOverEvent() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        var replay = new Replay {
            Players = [player1],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0)
                ]),
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), player1, 100)
                // No MatchOverReplayEvent
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.Multiple(() => {
            Assert.That(matchResult.IsValid, Is.False);
            Assert.That(matchResult.Concluded, Is.False);
        });
    }

    [Test]
    public void GetMatchResult_ReturnsInvalidMatchResult_WhenPlayerDataDoesNotMatchLobby() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        var replay = new Replay {
            Players = [player1],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0)
                ]),
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), [1000], [], [])
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(1, "different_player", "Different Player", false, false) // Different player data
        });

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.That(matchResult.IsValid, Is.False);
    }

    [Test]
    public void GetMatchResult_ReturnsValidMatchResult_WithCorrectPlayerStatistics() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        var replay = new Replay {
            Players = [player1],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0)
                ]),
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(60)), [1000], [], [
                    new MatchOverReplayEvent.PlayerStatistics(1000, 0, "Player 1", 0, 5)
                ])
            ],
            GameId = "TestGameId",
            Duration = TimeSpan.FromSeconds(60) // Set duration for the match
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false)
        });

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.Multiple(() => {
            Assert.That(matchResult.IsValid, Is.True);
            Assert.That(matchResult.MatchDuration, Is.EqualTo(TimeSpan.FromSeconds(60)));
            Assert.That(matchResult.Winners, Does.Contain("p1"));
            Assert.That(matchResult.Losers, Is.Empty);
        });
    }

    [Test]
    public void GetMatchResult_HandlesTimeSpanOverflow_WhenEventsAreOutOfOrder() {
        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        var replay = new Replay {
            Players = [player1],
            Events = [
                new MatchStartReplayEvent(TimeSpan.FromSeconds(2), "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0)
                ]),
                new MatchOverReplayEvent(TimeSpan.FromSeconds(1), [1000], [], []) // Earlier timestamp than start
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.That(matchResult.IsValid, Is.False);
    }

    [Test]
    public void GetMatchResult_ReturnsValidListOfPlayerEvents_WhenPlayersHaveEvents() {

        // Arrange
        ReplayPlayer player1 = new ReplayPlayer(1000, 0, "Player 1", 0, 0, "british_africa", "ai_default");
        ReplayPlayer player2 = new ReplayPlayer(1001, 1, "Player 2", 0, 0, "afrika_korps", "ai_default");
        var replay = new Replay {
            Players = [player1, player2],
            Events = [
                new MatchStartReplayEvent(TimeSpan.Zero, "TestId", "v1.0", "test_scenario", [
                    new MatchStartReplayEvent.PlayerData(1000, "Player 1", "company1", 0),
                    new MatchStartReplayEvent.PlayerData(1001, "Player 2", "company2", 1)
                ]),
                new SquadDeployedEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(1)), player1, 100),
                new SquadKilledEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(2)), player2, 200), // Technically makes this sequence invalid, but we want to test player events
                new MatchOverReplayEvent(TimeSpan.Zero.Add(TimeSpan.FromSeconds(3)), [1000], [1001], [])
            ],
            GameId = "TestGameId",
        };
        var result = new ReplayAnalysisResult {
            Replay = replay,
            GameId = replay.GameId,
        };
        var lobby = Substitute.For<ILobby>();
        lobby.Participants.Returns(new HashSet<Participant> {
            new Participant(0, "p1", "Player 1", false, false),
            new Participant(1, "p2", "Player 2", false, false)
        });

        // Act
        var matchResult = result.GetMatchResult(lobby);

        // Assert
        Assert.That(matchResult.CompanyModifiers, Has.Count.EqualTo(2));
        Assert.Multiple(() => {
            Assert.That(matchResult.CompanyModifiers["p1"], Has.Count.EqualTo(1)); // Player 1 has one events
            Assert.That(matchResult.CompanyModifiers["p2"], Has.Count.EqualTo(1)); // Player 2 has one event
        });

    }

}
