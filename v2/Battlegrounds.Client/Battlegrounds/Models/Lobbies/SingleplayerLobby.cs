using System.Threading.Channels;

using Battlegrounds.Facades.API;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Services;

using Serilog;

namespace Battlegrounds.Models.Lobbies;

public sealed class SingleplayerLobby : ILobby, IDisposable {

    private static readonly ILogger _logger = Log.ForContext<SingleplayerLobby>();

    private readonly ICompanyService _companyService;
    private readonly IBattlegroundsServerAPI _serverAPI;
    private readonly Channel<LobbyEvent> _internalEvents;
    private readonly HashSet<Participant> _participants = [];
    private readonly Dictionary<string, Company> _companies = [];
    private readonly List<LobbySetting> _settings = [
        new LobbySetting { Name = LobbySetting.SETTING_GAMEMODE, Type = LobbySettingType.Selection, Options = [
            new ("Domination", "domination"),
            new ("Victory Points", "victory_points")]
        },
        // TODO: More settings
    ];
    private readonly Participant _localParticipant;

    private Map _map;
    private bool _isActive = true;
    private bool disposedValue;
    private readonly Team _team1 = new Team(TeamType.Allies, "Allies", [
        new Team.Slot(0, null, "british_africa", string.Empty, AIDifficulty.HUMAN, false, false),
        new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
        new Team.Slot(2, null, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
        new Team.Slot(3, null, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
        ]);

    private readonly Team _team2 = new Team(TeamType.Axis, "Axis", [
        new Team.Slot(0, null, "afrika_korps", string.Empty, AIDifficulty.HARD, false, false),
        new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
        new Team.Slot(2, null, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
        new Team.Slot(3, null, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
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

    public SingleplayerLobby(string name, Game game, Map map, Participant localParticipant, IBattlegroundsServerAPI serverAPI, ICompanyService companyService) {
        Name = name;
        Game = game;

        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService), "Company service cannot be null");
        _serverAPI = serverAPI ?? throw new ArgumentNullException(nameof(serverAPI), "Server API cannot be null");

        _internalEvents = Channel.CreateUnbounded<LobbyEvent>();
        _map = map;
        _localParticipant = localParticipant;
        _participants.Add(localParticipant);

        Participant aiParticipant = new Participant(1, Guid.NewGuid().ToString(), "AI - Standard", true, true); // TODO: Make constructor caller handle this
        _participants.Add(aiParticipant);

        _team1.Slots[0] = _team1.Slots[0] with { ParticipantId = _localParticipant.ParticipantId };
        _team2.Slots[0] = _team2.Slots[0] with { ParticipantId = aiParticipant.ParticipantId };

    }

    public async ValueTask<LobbyEvent?> GetNextEvent() {
        try {
            return await _internalEvents.Reader.ReadAsync();
        } catch (ChannelClosedException) {
            return null; // Channel is closed, no more events (why must this be an exception...)
        }
    }

    public Task<LaunchGameResult> LaunchGame() => Task.FromResult(new LaunchGameResult()); // NOP operation in singleplayer mode

    public async ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult) {
        if (matchResult.Failed || matchResult.Replay is null) {
            return false; // Cannot report match result if it failed or replay is null
        }

        var result = matchResult.GetMatchResult(this);
        if (result == MatchResult.Unknown) {
            return false; // Cannot determine match result
        }

        if (!result.IsValid) {
            // Not valid, nothing more to do, no reason to report
            return false;
        }

        if (result.CompanyModifiers.TryGetValue(_localParticipant.ParticipantId, out var localEvents) && localEvents.Count == 0) {
            _logger.Warning("No events for local participant {ParticipantId} in match result", _localParticipant.ParticipantId);
            return false; // No events for local participant, nothing to report
        }

        // Apply the company changes to the local company
        Company? updatedCompany = await _companyService.ApplyEvents(localEvents, _companies[_localParticipant.ParticipantId], commitLocally: true);
        if (updatedCompany is null) {
            _logger.Error("Failed to apply company changes for local participant {ParticipantId} in match result", _localParticipant.ParticipantId);
            return false; // Failed to apply company changes, cannot report
        }

        result.LobbyId = "local-singleplayer"; // Set the lobby ID to indicate this is a local singleplayer match
        var reported = await _serverAPI.ReportMatchResults(result); // Report the match result to the server
        if (!reported) {
            _logger.Error("Failed to report match result for game {GameId} to the server", matchResult.GameId);
        } else {
            if (!await _companyService.SyncCompanyWithRemote(updatedCompany)) {
                _logger.Error("Failed to synchronize company {CompanyId} with remote server after match result report", updatedCompany.Id);
            } else {
                _logger.Information("Match result for game {GameId} reported successfully and company synchronized", matchResult.GameId);
            }
        }

        return true; // Done what we needed to do, match result reported if possible

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

    public async Task RemoveAI(Team team, int slotIndex) {
        var slot = team.Slots[slotIndex];
        if (slot.ParticipantId is null || slot.ParticipantId == _localParticipant.ParticipantId) {
            return; // Cannot remove local player or empty slot
        }
        _participants.RemoveWhere(x => x.ParticipantId == slot.ParticipantId && x.IsAIParticipant); // Remove the AI participant
        team.Slots[slotIndex] = slot with { ParticipantId = null, Difficulty = AIDifficulty.HUMAN, Locked = false, CompanyId = string.Empty };
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI
    }

    public async Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty) {
        if (difficulty == AIDifficulty.HUMAN) {
            await RemoveAI(team, slotIndex); // If setting to human, remove AI participant
            return;
        }
        var slot = team.Slots[slotIndex];
        Participant? participant = _participants.FirstOrDefault(x => x.ParticipantId == slot.ParticipantId);
        if (participant is null) {
            int id = team == _team1 ? slotIndex : (slotIndex + 4); // Slot index is 0-3 for team1 and 4-7 for team2
            participant = new Participant(id, Guid.CreateVersion7().ToString(), string.Empty, true, true);
            _participants.Add(participant);
        }
        team.Slots[slotIndex] = slot with { ParticipantId = participant.ParticipantId, Difficulty = difficulty, CompanyId = string.Empty, Locked = false };
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI
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
        var chatMessage = new ChatMessage(_localParticipant.ParticipantId, _localParticipant.ParticipantName, chatChannel, msg);
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, chatMessage)); // Notify the UI of message
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                _internalEvents.Writer.Complete();
            }
            _isActive = false; // Mark the lobby as inactive
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
