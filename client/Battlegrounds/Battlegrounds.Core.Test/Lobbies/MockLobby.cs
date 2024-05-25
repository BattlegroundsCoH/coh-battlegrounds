using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Scenarios;
using Battlegrounds.Core.Lobbies;
using Battlegrounds.Core.Test.Games.Scenarios;

namespace Battlegrounds.Core.Test.Lobbies;

public sealed class MockLobby(Guid guid, string name, string game, ILobbyPlayer localPlayer) : ILobby {

    public sealed class MockPlayer(ulong playerId, string displayName) : ILobbyPlayer {
        public string Name => displayName;

        public string Faction { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public ulong PlayerId => playerId;

        public string CompanyId { get; set; } = Guid.Empty.ToString();
        public MockPlayer Clone() => new MockPlayer(PlayerId, Name) { Faction = this.Faction, CompanyId = this.CompanyId, CompanyName = this.CompanyName };
    }

    public sealed class MockSlot : ILobbySlot {

        public bool IsVisible { get; set; }

        public bool IsLocked { get; set; }

        public MockPlayer? Player { get; set; }

        ILobbyPlayer? ILobbySlot.Player => this.Player;
        public AIDifficulty Difficulty { get; set; }
        public MockSlot Occupy(MockPlayer? player) {
            Player = player;
            Difficulty = AIDifficulty.AI_HUMAN;
            return this;
        }
    }

    public sealed class MockTeam(string alliance) : ILobbyTeam {

        public sealed class Builder {

            public MockSlot[] Slots = [
                new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = true, Player = null },
                new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = false, Player = null },
                new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = false, Player = null },
                new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = false, Player = null },
            ];

            public string Name { get; set; } = string.Empty;
            public string Alliance { get; set; } = string.Empty;
            public MockTeam Build() => new MockTeam(Alliance) { Name = Name, Slots = Slots };
            public Builder Slot(int index, Action<MockSlot> action) {
                action(Slots[index]);
                return this;
            }

        }

        private MockSlot[] slots = [
            new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = true, Player = null },
            new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = false, Player = null },
            new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = false, Player = null },
            new MockSlot { Difficulty = AIDifficulty.AI_HUMAN, IsLocked = false, IsVisible = false, Player = null },
        ];

        public required string Name { get; set; }

        public string Alliance => alliance;
        public MockSlot[] Slots { get => slots; set => slots = value; }
        ILobbySlot[] ILobbyTeam.Slots => slots;
    }

    public sealed class Builder {
        public string Game { get; set; } = CoH3.COH3_NAME;
        public IDictionary<ulong, ICompany> MatchCompanies { get; set; } = new Dictionary<ulong, ICompany>();
        public IScenario Scenario { get; set; } = ScenarioFixture.desert_village_2p_mkiii;
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        public MockTeam.Builder Team1 { get; set; } = new MockTeam.Builder { Alliance = "allies", Name = "allies" };
        public MockTeam.Builder Team2 { get; set; } = new MockTeam.Builder { Alliance = "axis", Name = "axis" };
    }

    private readonly ILobbyPlayer _localPlayer = localPlayer;

    private readonly Dictionary<string, string> settings = [];
    private readonly HashSet<ulong> readyPlayers = [];

    private MockTeam team1 = new MockTeam("allies") { Name = "allies" };
    private MockTeam team2 = new MockTeam("axis") { Name = "axis" };

    private IScenario scenario = ScenarioFixture.desert_village_2p_mkiii;

    public IDictionary<ulong, ICompany> MatchCompanies { get; init; } = new Dictionary<ulong, ICompany>();

    public Guid Guid => guid;

    public string Name => name;

    public string Game => game;

    public bool IsHost { get; set; }

    public MockTeam Team1 { get => team1; set => team1 = value; }

    public MockTeam Team2 { get => team2; set => team2 = value; }

    ILobbyTeam ILobby.Team1 => team1;

    ILobbyTeam ILobby.Team2 => team2;

    public IDictionary<string, string> Settings { get => settings; init => settings = new Dictionary<string, string>(value); }

    public ISet<ulong> ReadyPlayers => readyPlayers;

    public ulong LocalPlayerId => _localPlayer.PlayerId;

    public IScenario Scenario { get => scenario; init => scenario = value; }

    public Task<bool> LaunchMatchAsync() {
        throw new NotImplementedException();
    }

    public Task Leave() {
        throw new NotImplementedException();
    }

    public Task MoveToSlot(int team, int slot) {
        throw new NotImplementedException();
    }

    public void SetChatCallback(Action<ILobbyChatMessage> callback) {
        throw new NotImplementedException();
    }

    public Task SetCompany(int team, int slot, string company) {
        throw new NotImplementedException();
    }

    public Task SetDifficulty(int team, int slot, int aiDifficulty) {
        throw new NotImplementedException();
    }

    public Task SetLocked(int team, int slot, bool isLocked) {
        throw new NotImplementedException();
    }

    public Task SetScenario(IScenario scenario) {
        throw new NotImplementedException();
    }

    public Task SetSetting(string setting, string value) {
        throw new NotImplementedException();
    }

    public Task SetState(bool ready) {
        throw new NotImplementedException();
    }

    public Task SetTeamNames(string team1, string team2) {
        throw new NotImplementedException();
    }

    public void SetUpdateCallback(Action callback) {
        throw new NotImplementedException();
    }

    public Task<bool> UploadCompanyAsync(ICompany company) {
        throw new NotImplementedException();
    }

    public Task<bool> UploadGamemodeAsync(Stream inputStream) {
        throw new NotImplementedException();
    }

    public Task<IDictionary<ulong, ICompany>> DownloadCompaniesAsync() => Task.FromResult<IDictionary<ulong, ICompany>>(
        MatchCompanies.Where(x => x.Key != _localPlayer.PlayerId).ToDictionary()
    );

}
