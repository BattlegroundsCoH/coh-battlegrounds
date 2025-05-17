using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

public sealed class SingleplayerLobby : ILobby {

    private readonly Channel<LobbyEvent> _internalEvents;
    private readonly HashSet<Participant> _participants = [];
    private readonly Dictionary<string, Company> _companies = [];
    private readonly Dictionary<string, string> _settings = [];
    private readonly Participant _localParticipant;

    private Map _map = null!;
    private bool _isActive = true;

    private readonly Team _team1 = new Team(TeamType.Allies, "Allies", [
        new Team.Slot(0, null, "british_africa", string.Empty, string.Empty, false, false),
        new Team.Slot(1, null, string.Empty, string.Empty, string.Empty, true, false),
        new Team.Slot(2, null, string.Empty, string.Empty, string.Empty, true, false),
        new Team.Slot(3, null, string.Empty, string.Empty, string.Empty, true, false),
        ]);

    private readonly Team _team2 = new Team(TeamType.Axis, "Axis", [
        new Team.Slot(0, null, "afrika_korps", string.Empty, "Expert", false, false),
        new Team.Slot(1, null, string.Empty, string.Empty, string.Empty, true, false),
        new Team.Slot(2, null, string.Empty, string.Empty, string.Empty, true, false),
        new Team.Slot(3, null, string.Empty, string.Empty, string.Empty, true, false),
        ]);

    public string Name { get; }

    public bool IsHost => true;

    public bool IsActive => _isActive;

    public ISet<Participant> Participants => _participants;

    public Dictionary<string, Company> Companies => _companies;

    public Team Team1 => _team1;

    public Team Team2 => _team2;

    public Game Game { get; }

    public SingleplayerLobby(string name, Game game, Participant localParticipant) {
        Name = name;
        Game = game;

        _internalEvents = Channel.CreateUnbounded<LobbyEvent>();
        _localParticipant = localParticipant;
        _participants.Add(localParticipant);

        _team1.Slots[0] = _team1.Slots[0] with { ParticipantId = _localParticipant.ParticipantId };

    }

    public async ValueTask<LobbyEvent?> GetNextEvent() {
        return await _internalEvents.Reader.ReadAsync();
    }

    public Task<LaunchGameResult> LaunchGame() => Task.FromResult(new LaunchGameResult()); // NOP operation in singleplayer mode

    public Task ReportMatchResult(ReplayAnalysisResult matchResult) {
        throw new NotImplementedException();
    }

    public Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) => Task.FromResult(new UploadGamemodeResult()); // NOP operation in singleplayer mode

    public (Team? team, int slotId) GetLocalPlayerSlot() {
        var id = Array.FindIndex(_team1.Slots, x => x.ParticipantId == _localParticipant.ParticipantId);
        if (id != -1) {
            return (_team1, id);
        }
        id = Array.FindIndex(_team2.Slots, x => x.ParticipantId == _localParticipant.ParticipantId);
        if (id != -1) {
            return (_team2, id);
        }
        return (null, -1);
    }

    public Task SetCompany(Team team, int slotId, string id) {
        team.Slots[slotId] = team.Slots[slotId] with { CompanyId = id };
        return Task.CompletedTask;
    }

}
