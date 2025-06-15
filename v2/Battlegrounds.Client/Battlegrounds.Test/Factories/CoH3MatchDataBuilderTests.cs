using Battlegrounds.Factories;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Test.Models.Blueprints;
using Battlegrounds.Test.Models.Playing;

namespace Battlegrounds.Test.Factories;

[TestFixture]
public class CoH3MatchDataBuilderTests {
    private MockLobby _mockLobby;
    private MockCoH3Game _mockGame;
    private CoH3MatchDataBuilder _builder;
    private string _testFilePath;

    [SetUp]
    public void Setup() {
        _mockLobby = new MockLobby();
        _mockGame = new MockCoH3Game();
        _builder = new CoH3MatchDataBuilder(_mockLobby, _mockGame);

        // Create a temporary file path for testing
        _testFilePath = Path.Combine(Path.GetTempPath(), "CoH3MatchDataTest.lua");
        _mockGame.MatchDataPath = _testFilePath;

        // Delete the file if it exists from a previous test
        if (File.Exists(_testFilePath)) {
            File.Delete(_testFilePath);
        }
    }

    [TearDown]
    public void TearDown() {
        // Clean up the test file
        if (File.Exists(_testFilePath)) {
            File.Delete(_testFilePath);
        }
    }

    [Test]
    public async Task BuildMatchData_WithTeamSetup_ReturnsCorrectLuaData() {
        // Act
        string result = await _builder.BuildMatchData();

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("teams = {"));
        Assert.That(result, Does.Contain("team = 1"));
        Assert.That(result, Does.Contain("team = 2"));
        Assert.That(result, Does.Contain("team_name = \"Team 1\""));
        Assert.That(result, Does.Contain("team_name = \"Team 2\""));
    }

    [Test]
    public async Task WriteMatchData_CreatesFile() {
        // Arrange
        string matchData = "test = { key = \"value\" }";

        // Act
        bool result = await _builder.WriteMatchData(matchData);

        // Assert
        Assert.Multiple(() => {
            Assert.That(result, Is.True);
            Assert.That(File.Exists(_testFilePath), Is.True);
        });

        string fileContent = await File.ReadAllTextAsync(_testFilePath);
        Assert.That(fileContent, Is.EqualTo(matchData));
    }

    [Test]
    public void WriteMatchData_WithIOException_ThrowsException() {

        // Arrange
        _mockGame.MatchDataPath = "Z:\\invalid\\path\\that\\doesnt\\exist.lua";

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _builder.WriteMatchData("test data"));

        Assert.That(ex.Message, Does.Contain("Failed to create match data file"));

    }

    [Test]
    public async Task WriteMatchData_IncludesTeamData() {
        // Act
        bool result = await _builder.WriteMatchData(await _builder.BuildMatchData());

        // Assert
        Assert.Multiple(() => {
            Assert.That(result, Is.True);
            Assert.That(File.Exists(_testFilePath), Is.True);
        });

        string fileContent = await File.ReadAllTextAsync(_testFilePath);
        // We ignore the dummy content as the method will generate its own content
        Assert.That(fileContent, Does.Contain("teams = {"));
    }

    [Test]
    public async Task WriteMatchData_IncludesCompanyData() {
        // Act
        bool result = await _builder.WriteMatchData(await _builder.BuildMatchData());

        // Assert
        Assert.Multiple(() => {
            Assert.That(result, Is.True);
            Assert.That(File.Exists(_testFilePath), Is.True);
        });

        string fileContent = await File.ReadAllTextAsync(_testFilePath);
        Assert.That(fileContent, Does.Contain("companies = {"));
        Assert.That(fileContent, Does.Contain("[\"company1\"] = {"));
        Assert.That(fileContent, Does.Contain("name = \"Test Company 1\""));
    }

    [Test]
    public void BuildMatchData_WithMissingParticipant_ThrowsException() {
        // Arrange
        _mockLobby.Team1.Slots[0] = _mockLobby.Team1.Slots[0] with { ParticipantId = "non_existent_id" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(_builder.BuildMatchData);

        Assert.That(ex.Message, Does.Contain("Unable to find participant with ID"));
    }

    #region Mock Classes

    // These mock classes simulate the dependencies of CoH3MatchDataBuilder for testing

    private class MockLobby : ILobby {
        public string Name { get; } = "Test Lobby";
        public bool IsHost { get; } = true;
        public bool IsActive { get; } = true;
        public Game Game { get; } = null!;  // Not used in tests

        public ISet<Participant> Participants { get; } = new HashSet<Participant> {
            new(0, "player1", "Player 1", false, true),
            new(1, "player2", "Player 2", false, true),
            new(2, "player3", "Player 3", false, true),
            new(3, "player4", "Player 4", false, true)
        };

        public Dictionary<string, Company> Companies { get; } = new() {
            {
                "company1",
                new Company
                {
                    Id = "company1",
                    Name = "Test Company 1",
                    Squads =
                    [
                        new()
                        {
                            Id = 1,
                            Name = "Test Squad",
                            Experience = 100,
                            Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK,
                            Upgrades = [UpgradeBlueprintFixture.UPG_LMG_BREN]
                        }
                    ]
                }
            },
            {
                "company2",
                new Company
                {
                    Id = "company2",
                    Name = "Test Company 2",
                    Squads = []
                }
            }
        };

        public Team Team1 { get; } = new(
            TeamType.Axis,
            "Team 1",
            [
                new Team.Slot(1, "player1", "german", "company1", AIDifficulty.HUMAN, false, false),
                new Team.Slot(2, "player2", "german", "company2", AIDifficulty.EASY, false, false)
            ]
        );

        public Team Team2 { get; } = new(
            TeamType.Allies,
            "Team 2",
            [
                new Team.Slot(3, "player3", "american", "company2", AIDifficulty.HARD, false, false),
                new Team.Slot(4, "player4", "british", "company2", AIDifficulty.EXPERT, false, false)
            ]
        );

        public IList<LobbySetting> Settings => throw new NotImplementedException();

        public Map Map => throw new NotImplementedException();

        public string? GetLocalPlayerId() {
            throw new NotImplementedException();
        }

        public (Team? team, int slotId) GetLocalPlayerSlot() {
            throw new NotImplementedException();
        }

        public ValueTask<LobbyEvent?> GetNextEvent() =>
            ValueTask.FromResult<LobbyEvent?>(null);

        public Task<LaunchGameResult> LaunchGame() =>
            Task.FromResult(new LaunchGameResult());

        public Task RemoveAI(Team team, int slotIndex) {
            throw new NotImplementedException();
        }

        public ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult) => new ValueTask<bool>(true);

        public Task SendMessage(string channel, string msg) {
            throw new NotImplementedException();
        }

        public Task SetCompany(Team team, int slotId, string id) {
            throw new NotImplementedException();
        }

        public Task<bool> SetMap(Map map) {
            throw new NotImplementedException();
        }

        public Task SetSetting(LobbySetting newSetting) {
            throw new NotImplementedException();
        }

        public Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty) {
            throw new NotImplementedException();
        }

        public Task ToggleSlotLock(Team team, int slotIndex) {
            throw new NotImplementedException();
        }

        public Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) =>
            Task.FromResult(new UploadGamemodeResult());
    }

    #endregion
}
