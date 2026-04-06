using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;
using Battlegrounds.Test.Models.Blueprints;

using NSubstitute;

namespace Battlegrounds.Test.Models.Playing;

[TestFixture]
public class SimulatedAppInstanceTests {

    private readonly CoH3ReplayParser _parser = new();

    [Test]
    public async Task WaitForMatch_GeneratesReplayWithLobbyPlayersAndValidMatchResult() {
        var game = CreateGame();
        var map = new Map("Pachino", "Test Map", 2, string.Empty, "pachino_2p");
        var participants = new HashSet<Participant> {
            new(0, "local-player", "CoDiEx", false, true),
            new(1, "ai-player", "Computer", true, true)
        };

        var localCompany = CreateCompany(
            "company-local",
            "Allied Company",
            "british_africa",
            CreateSquad(11, SquadBlueprintFixture.SBP_TOMMY_UK, 120f, 2, 0),
            CreateSquad(12, SquadBlueprintFixture.SBP_HALFTRACK_M3_UK, 45f, 1, 1));

        var aiCompany = CreateCompany(
            "company-ai",
            "Axis Company",
            "afrika_korps",
            CreateSquad(21, SquadBlueprintFixture.SBP_PANZERGRENADIER_AK, 15f, 0, 0));

        var lobby = CreateLobby(
            map,
            participants,
            new Team(TeamType.Allies, "Allies", [
                new Team.Slot(0, "local-player", "british_africa", localCompany.Id, AIDifficulty.HUMAN, false, false)
            ]),
            new Team(TeamType.Axis, "Axis", [
                new Team.Slot(0, "ai-player", "afrika_korps", aiCompany.Id, AIDifficulty.NORMAL, false, false)
            ]),
            new Dictionary<string, Company> {
                [localCompany.Id] = localCompany,
                [aiCompany.Id] = aiCompany,
            },
            "local-player");

        var instance = new SimulatedAppInstance(game, true, new SimulatedAppRunParameters {
            SimulateAppRunTime = TimeSpan.Zero
        }, lobby);

        string replayPath = string.Empty;
        try {
            var playResult = await instance.WaitForMatch();
            replayPath = playResult.ReplayFilePath;

            Assert.That(File.Exists(replayPath), Is.True, "Generated replay file should exist.");

            var replay = _parser.ParseReplayFile(replayPath);
            var matchStart = replay.Events.OfType<MatchStartReplayEvent>().Single();
            var matchOver = replay.Events.OfType<MatchOverReplayEvent>().Single();
            var analysis = new ReplayAnalysisResult {
                GameId = replay.GameId,
                Replay = replay,
            };
            var matchResult = analysis.GetMatchResult(lobby);

            using (Assert.EnterMultipleScope()) {
                Assert.That(replay.GameId, Is.EqualTo(CoH3.GameId));
                Assert.That(replay.Players.Select(x => x.PlayerName), Is.EqualTo(new[] { "CoDiEx", "Computer" }));
                Assert.That(matchStart.Scenario, Is.EqualTo(map.ScenarioName));
                Assert.That(matchStart.Players, Has.Count.EqualTo(2));
                Assert.That(matchStart.Players.Single(x => x.PlayerId == 1000).CompanyId, Is.EqualTo(localCompany.Id));
                Assert.That(matchStart.Players.Single(x => x.PlayerId == 1000).ModId, Is.EqualTo(0));
                Assert.That(matchStart.Players.Single(x => x.PlayerId == 1001).CompanyId, Is.EqualTo(aiCompany.Id));
                Assert.That(matchStart.Players.Single(x => x.PlayerId == 1001).ModId, Is.EqualTo(1));
                Assert.That(replay.Events.OfType<SquadDeployedEvent>().Count(), Is.EqualTo(3));
                Assert.That(replay.Events.OfType<SquadRecalledEvent>().Count(), Is.EqualTo(2));
                Assert.That(replay.Events.OfType<SquadKilledEvent>().Count(), Is.EqualTo(1));
                Assert.That(matchOver.Winners, Is.EqualTo(new List<int> { 1000 }));
                Assert.That(matchOver.Losers, Is.EqualTo(new List<int> { 1001 }));
                Assert.That(matchResult.IsValid, Is.True);
                Assert.That(matchResult.Concluded, Is.True);
                Assert.That(matchResult.Winners, Contains.Item("local-player"));
                Assert.That(matchResult.Losers, Contains.Item("ai-player"));
                Assert.That(matchResult.PlayerCompanies["local-player"], Is.EqualTo(localCompany.Id));
                Assert.That(matchResult.PlayerCompanies["ai-player"], Is.EqualTo(aiCompany.Id));
                Assert.That(matchResult.CompanyModifiers["local-player"], Has.Count.GreaterThan(0));
                Assert.That(matchResult.BadEvents, Is.Empty);
            }
        } finally {
            DeleteIfExists(replayPath);
        }
    }

    [Test]
    public async Task WaitForMatch_ExcludesHiddenAndLockedSlotsFromGeneratedReplay() {
        var game = CreateGame();
        var map = new Map("Pachino", "Test Map", 4, string.Empty, "pachino_2p");
        var participants = new HashSet<Participant> {
            new(0, "local-player", "CoDiEx", false, true),
            new(1, "ai-player", "Computer", true, true),
            new(2, "hidden-player", "Hidden Player", false, true),
            new(3, "locked-player", "Locked Player", false, true)
        };

        var visibleCompany = CreateCompany("company-visible", "Visible Company", "british_africa", CreateSquad(11, SquadBlueprintFixture.SBP_TOMMY_UK));
        var aiCompany = CreateCompany("company-ai", "AI Company", "afrika_korps", CreateSquad(21, SquadBlueprintFixture.SBP_PANZERGRENADIER_AK));
        var hiddenCompany = CreateCompany("company-hidden", "Hidden Company", "british_africa", CreateSquad(31, SquadBlueprintFixture.SBP_TOMMY_UK));
        var lockedCompany = CreateCompany("company-locked", "Locked Company", "afrika_korps", CreateSquad(41, SquadBlueprintFixture.SBP_PANZERGRENADIER_AK));

        var lobby = CreateLobby(
            map,
            participants,
            new Team(TeamType.Allies, "Allies", [
                new Team.Slot(0, "local-player", "british_africa", visibleCompany.Id, AIDifficulty.HUMAN, false, false),
                new Team.Slot(1, "hidden-player", "british_africa", hiddenCompany.Id, AIDifficulty.HUMAN, true, false)
            ]),
            new Team(TeamType.Axis, "Axis", [
                new Team.Slot(0, "ai-player", "afrika_korps", aiCompany.Id, AIDifficulty.NORMAL, false, false),
                new Team.Slot(1, "locked-player", "afrika_korps", lockedCompany.Id, AIDifficulty.HUMAN, false, true)
            ]),
            new Dictionary<string, Company> {
                [visibleCompany.Id] = visibleCompany,
                [aiCompany.Id] = aiCompany,
                [hiddenCompany.Id] = hiddenCompany,
                [lockedCompany.Id] = lockedCompany,
            },
            "local-player");

        var instance = new SimulatedAppInstance(game, true, new SimulatedAppRunParameters {
            SimulateAppRunTime = TimeSpan.Zero
        }, lobby);

        string replayPath = string.Empty;
        try {
            var playResult = await instance.WaitForMatch();
            replayPath = playResult.ReplayFilePath;

            var replay = _parser.ParseReplayFile(replayPath);
            var matchStart = replay.Events.OfType<MatchStartReplayEvent>().Single();
            string[] playerNames = [.. replay.Players.Select(x => x.PlayerName)];

            using (Assert.EnterMultipleScope()) {
                Assert.That(replay.Players, Has.Count.EqualTo(2));
                Assert.That(playerNames, Is.EqualTo(new[] { "CoDiEx", "Computer" }));
                Assert.That(playerNames, Does.Not.Contain("Hidden Player"));
                Assert.That(playerNames, Does.Not.Contain("Locked Player"));
                Assert.That(matchStart.Players.Select(x => x.CompanyId), Is.EqualTo(new[] { visibleCompany.Id, aiCompany.Id }));
            }
        } finally {
            DeleteIfExists(replayPath);
        }
    }

    private static ILobby CreateLobby(Map map, ISet<Participant> participants, Team team1, Team team2, Dictionary<string, Company> companies, string? localPlayerId) {
        var lobby = Substitute.For<ILobby>();
        lobby.Name.Returns("Simulated Test Lobby");
        lobby.Participants.Returns(participants);
        lobby.Team1.Returns(team1);
        lobby.Team2.Returns(team2);
        lobby.Companies.Returns(companies);
        lobby.Map.Returns(map);
        lobby.GetLocalPlayerId().Returns(localPlayerId);
        return lobby;
    }

    private static CoH3 CreateGame() {
        var configuration = new Configuration {
            CoH3 = new Configuration.CoH3Configuration {
                ModProjectPath = Path.Combine(Path.GetTempPath(), "bg_simulated_match.coh3mod")
            }
        };
        return new CoH3(configuration);
    }

    private static Company CreateCompany(string id, string name, string faction, params Squad[] squads) => new() {
        Id = id,
        Name = name,
        Faction = faction,
        GameId = CoH3.GameId,
        Squads = squads,
    };

    private static Squad CreateSquad(int id, Battlegrounds.Models.Blueprints.SquadBlueprint blueprint, float experience = 0f, int infantryKills = 0, int vehicleKills = 0) => new() {
        Id = id,
        Blueprint = blueprint,
        Experience = experience,
        TotalInfantryKills = infantryKills,
        TotalVehicleKills = vehicleKills,
    };

    private static void DeleteIfExists(string path) {
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) {
            File.Delete(path);
        }
    }

}
