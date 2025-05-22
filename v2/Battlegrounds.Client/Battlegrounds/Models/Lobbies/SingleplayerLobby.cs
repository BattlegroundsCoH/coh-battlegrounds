using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

public sealed class SingleplayerLobby : ILobby {

    private readonly Channel<LobbyEvent> _internalEvents;
    private readonly HashSet<Participant> _participants = [];
    private readonly Dictionary<string, Company> _companies = [];
    private readonly List<LobbySetting> _settings = [
        new LobbySetting { Name = LobbySetting.SETTING_GAMEMODE, Type = LobbySettingType.Selection, Options = [
            new ("Domunation", "domination"),
            new ("Victory Points", "victory_points")]
        },
        // TODO: More settings
    ];
    private readonly Participant _localParticipant;

    private Map _map;
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

    public IList<LobbySetting> Settings => _settings;

    public Map Map => _map;

    public SingleplayerLobby(string name, Game game, Map map, Participant localParticipant) {
        Name = name;
        Game = game;

        _internalEvents = Channel.CreateUnbounded<LobbyEvent>();
        _map = map;
        _localParticipant = localParticipant;
        _participants.Add(localParticipant);

        Participant aiParticipant = new Participant(Guid.NewGuid().ToString(), "AI - Standard", true); // TODO: Make constructor caller handle this
        _participants.Add(aiParticipant);

        _team1.Slots[0] = _team1.Slots[0] with { ParticipantId = _localParticipant.ParticipantId };
        _team2.Slots[0] = _team2.Slots[0] with { ParticipantId = aiParticipant.ParticipantId };

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

    public async Task SetCompany(Team team, int slotId, string id) {
        team.Slots[slotId] = team.Slots[slotId] with { CompanyId = id };
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI
    }

    public string? GetLocalPlayerId() => _localParticipant.ParticipantId;

    public Task RemoveAI(Team team, int slotIndex) {
        throw new NotImplementedException();
    }

    public Task SetSlotAIDifficulty(Team team, int slotIndex, string difficulty) {
        throw new NotImplementedException();
    }

    public async Task ToggleSlotLock(Team team, int slotIndex) {
        team.Slots[slotIndex] = team.Slots[slotIndex] with { Locked = !team.Slots[slotIndex].Locked };
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI
    }

    public async Task<bool> SetMap(Map map) {
        if (_map == map) {
            return true; // Map is already set
        }
        if (map.MaxPlayers != _map.MaxPlayers) {
            if (map.MaxPlayers < _participants.Count) {
                return false; // Cannot set map with less players than current participant count
            }
            int slotsPerTeam = map.MaxPlayers / 2;
            for (int i = 0; i < _team1.Slots.Length; i++) { // TODO: More graceful handling (ie. if a player is in slot 3 and only 2 slots are available, move them to slot 1 or 2)
                var isHidden = i >= slotsPerTeam;
                _team1.Slots[i] = _team1.Slots[i] with { Hidden = isHidden };
                _team2.Slots[i] = _team2.Slots[i] with { Hidden = isHidden };
            }
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated)); // Notify the UI of changes to teams
        }
        _map = map; // Set the new map
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.MapUpdated, map)); // Notify the UI of map change
        return true; // Map set successfully
    }

    public async Task SetSetting(LobbySetting newSetting) {
        int indexOfSetting = _settings.FindIndex(x => x.Name == newSetting.Name);
        if (newSetting.Name == LobbySetting.SETTING_GAMEMODE) {
            // TODO: Handle gamemode change
            // ie. if gamemode == victory_points add a new setting for specifying amount of victory points (250, 500, etc)
        }
        if (indexOfSetting != -1) { // Swapping existing setting
            _settings[indexOfSetting] = newSetting;
        } else {
            _settings.Add(newSetting);
        }
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SettingUpdated)); // Notify the UI of setting change
    }

    public async Task SendMessage(string channel, string msg) {
        var chatChannel = channel switch {
            "team" => ChatChannel.Team,
            "all" => ChatChannel.All,
            _ => throw new ArgumentException($"Invalid chat channel: {channel}")
        };
        var chatMessage = new ChatMessage(_localParticipant.ParticipantName, chatChannel, msg);
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, chatMessage)); // Notify the UI of message
    }

}
