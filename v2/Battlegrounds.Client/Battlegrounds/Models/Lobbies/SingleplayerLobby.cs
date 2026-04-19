using System.Threading.Channels;

using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Services;

using Serilog;

namespace Battlegrounds.Models.Lobbies;

public sealed class SingleplayerLobby(LobbySetup lobbySetup, IBattlegroundsServerAPI serverAPI, ICompanyService companyService) : ILobby, IDisposable {

    private static readonly ILogger _logger = Log.ForContext<SingleplayerLobby>();

    private readonly ICompanyService _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService), "Company service cannot be null");
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI ?? throw new ArgumentNullException(nameof(serverAPI), "Server API cannot be null");
    private readonly Channel<LobbyEvent> _internalEvents = Channel.CreateUnbounded<LobbyEvent>();
    private readonly HashSet<Participant> _participants = lobbySetup.Participants;
    private readonly Dictionary<string, Company> _companies = [];
    private readonly List<LobbySetting> _settings = lobbySetup.Settings;
    private readonly Participant _localParticipant = lobbySetup.Self;
    private readonly Team _team1 = lobbySetup.Team1;
    private readonly Team _team2 = lobbySetup.Team2;

    private Map _map = lobbySetup.Map;
    private bool _isActive = true;
    private bool _disposedValue;

    private MatchResult? _lastMatchResult;
    private Dictionary<string, Company>? _lastMatchCompanies;

    public string Name { get; } = lobbySetup.Name;

    public bool IsHost => true;

    public bool IsActive => _isActive;

    public ISet<Participant> Participants => _participants;

    public Dictionary<string, Company> Companies => _companies;

    public Team Team1 => _team1;

    public Team Team2 => _team2;

    public Game Game { get; } = lobbySetup.Game;

    public IList<LobbySetting> Settings => _settings;

    public Map Map => _map;

    public bool IsReady => true; // Always ready in singleplayer mode

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
            _logger.Error("Match result for game {GameId} failed or replay is null", matchResult.GameId);
            return false; // Cannot report match result if it failed or replay is null
        }

        _lastMatchCompanies = Companies.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Store the companies before applying changes so we can include them in the match result report
        _lastMatchResult = matchResult.GetMatchResult(this);
        if (_lastMatchResult == MatchResult.Unknown) {
            _logger.Error("Match result for game {GameId} could not be determined", matchResult.GameId);
            return false; // Cannot determine match result
        }

        if (!_lastMatchResult.IsValid) {
            _logger.Error("Match result for game {GameId} is invalid", matchResult.GameId);
            return false;
        }

        if (_lastMatchResult.CompanyModifiers.TryGetValue(_localParticipant.ParticipantId, out var localEvents) && localEvents.Count == 0) {
            _logger.Warning("No events for local participant {ParticipantId} in match result", _localParticipant.ParticipantId);
            return false; // No events for local participant, nothing to report
        }

        var localCompanyId = _lastMatchResult.PlayerCompanies[_localParticipant.ParticipantId];
        _logger.Information("Reporting match result for game {GameId} with local company ID {CompanyId}", matchResult.GameId, localCompanyId);

        // Apply the company changes to the local company
        Company? updatedCompany = await _companyService.ApplyEvents(localEvents, _companies[localCompanyId], commitLocally: true);
        if (updatedCompany is null) {
            _logger.Error("Failed to apply company changes for local participant {ParticipantId} in match result", _localParticipant.ParticipantId);
            return false; // Failed to apply company changes, cannot report
        }

        _lastMatchResult.LobbyId = "local-singleplayer"; // Set the lobby ID to indicate this is a local singleplayer match
        var reported = await _serverAPI.ReportMatchResults(_lastMatchResult); // Report the match result to the server
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

    public ValueTask<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) => ValueTask.FromResult(new UploadGamemodeResult() { Failed = false }); // NOP operation in singleplayer mode

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

    public async Task SetCompany(Team team, int slotId, string id, string faction) {
        team.Slots[slotId] = team.Slots[slotId] with { CompanyId = id, Faction = faction };
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

    public async Task SendMessage(ChatChannel channel, string msg) {
        var chatMessage = new ChatMessage(_localParticipant.ParticipantId, _localParticipant.ParticipantName, channel, msg);
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, chatMessage)); // Notify the UI of message
    }

    private void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                _internalEvents.Writer.Complete();
            }
            _isActive = false; // Mark the lobby as inactive
            _disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public ValueTask<bool> WaitForAllPlayersHaveGamemode() => ValueTask.FromResult(true); // NOP operation in singleplayer mode, always returns true since there's only one player

    public Participant? GetParticipant(string participantId) => _participants.FirstOrDefault(p => p.ParticipantId == participantId);

    public int GetRealPlayersCount() => 1; // Always 1 real player in singleplayer mode

    public Task BeginMatch() => Task.CompletedTask; // NOP operation in singleplayer mode, match begins immediately when game is launched

    public async Task EndMatch(EndMatchReason reason) {
        if (reason is EndMatchReason.MatchEndedInSuccess) {
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.MatchOver, reason)); // Notify the UI of match end
        }
    }

    public ValueTask PublishSystemMessage(string message) => ValueTask.CompletedTask; // NOP operation in singleplayer mode, system messages are not needed

    public Task SetSlotFaction(Team team, int slotIndex, string? faction) => Task.CompletedTask; // NOP operation in singleplayer mode, faction changes are not needed

    public Task MarkReady(bool isReady) => Task.CompletedTask; // NOP operation in singleplayer mode, player is always ready

    public Task KickPlayer(Team team, int slotIndex) => Task.CompletedTask; // NOP operation in singleplayer mode, cannot kick players

    public Task<MatchOverData?> GetMatchResults() {
        if (_lastMatchResult is null) {
            _logger.Warning("No match result available for game {GameId}, cannot generate match over data", Game.Id);
            return Task.FromResult<MatchOverData?>(null); // No match result available
        }
        if (_lastMatchCompanies is null) {
            _logger.Error("Last match result is available but last match companies are not, cannot generate match over data");
            return Task.FromResult<MatchOverData?>(null); // Cannot generate match over data without companies
        }
        return Task.FromResult<MatchOverData?>(MatchOverData.FromMatchResultForPlayer(_lastMatchResult, _localParticipant.ParticipantId, _lastMatchCompanies));
    }

    public async Task MoveToSlot(Team team, int slotIndex) {
        var (currentTeam, currentSlot) = GetLocalPlayerSlot();
        if (currentTeam == team && currentSlot == slotIndex) {
            return; // Already in the desired slot
        }

        // Assign the local participant to the new slot
        team.Slots[slotIndex] = team.Slots[slotIndex] with { ParticipantId = _localParticipant.ParticipantId, Difficulty = AIDifficulty.HUMAN, CompanyId = string.Empty, Locked = false };

        // Clear the old slot
        if (currentTeam is not null && currentSlot != -1) {
            currentTeam.Slots[currentSlot] = currentTeam.Slots[currentSlot] with { ParticipantId = null, Difficulty = AIDifficulty.HUMAN, CompanyId = string.Empty, Locked = false };
        }

        // Trigger team updated event to notify the UI of the slot change
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI of team changes

        if (currentTeam is not null && currentTeam != team) {
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, currentTeam.TeamType)); // Notify the UI of team changes
        }

    }

}
