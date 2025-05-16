using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

public sealed class SingleplayerLobby : ILobby {

    private readonly Channel<LobbyEvent> _internalEvents;
    private readonly HashSet<Participant> _participants = [];
    private readonly Dictionary<string, Company> _companies = [];
    private bool _isActive = true;

    private readonly Team _team1 = new Team(TeamType.Allies, "Allies", [
        new Team.Slot(0, null, "US", string.Empty, "Easy", false, false),
        new Team.Slot(1, null, "UK", string.Empty, "Easy", false, false),
        new Team.Slot(2, null, "US", string.Empty, "Easy", false, false),
        new Team.Slot(3, null, "UK", string.Empty, "Easy", false, false),
        ]);

    private readonly Team _team2 = new Team(TeamType.Axis, "Axis", [
        new Team.Slot(0, null, "Germany", string.Empty, "Easy", false, false),
        new Team.Slot(1, null, "Germany", string.Empty, "Easy", false, false),
        new Team.Slot(2, null, "Germany", string.Empty, "Easy", false, false),
        new Team.Slot(3, null, "Germany", string.Empty, "Easy", false, false),
        ]);

    public string Name { get; }

    public bool IsHost => true;

    public bool IsActive => _isActive;

    public ISet<Participant> Participants => _participants;

    public Dictionary<string, Company> Companies => _companies;

    public Team Team1 => _team1;

    public Team Team2 => _team2;

    public Game Game { get; }

    public SingleplayerLobby(string name, Game game) {
        Name = name;
        Game = game;

        _internalEvents = Channel.CreateUnbounded<LobbyEvent>();

    }

    public async ValueTask<LobbyEvent?> GetNextEvent() {
        return await _internalEvents.Reader.ReadAsync();
    }

    public Task<LaunchGameResult> LaunchGame() => Task.FromResult(new LaunchGameResult()); // NOP operation in singleplayer mode

    public Task ReportMatchResult(ReplayAnalysisResult matchResult) {
        throw new NotImplementedException();
    }

    public Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) => Task.FromResult(new UploadGamemodeResult()); // NOP operation in singleplayer mode

}
