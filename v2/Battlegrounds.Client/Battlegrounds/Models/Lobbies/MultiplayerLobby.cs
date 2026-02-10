using System.Threading.Channels;

using Battlegrounds.Facades.API;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Services;

using Grpc.Core;

using Serilog;

using LobbySetup = Battlegrounds.Factories.LobbySetup;

namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents a multiplayer lobby that manages participants, teams, game settings, and real-time state synchronization
/// for a battlegrounds session.
/// </summary>
/// <remarks>The MultiplayerLobby class is designed for real-time multiplayer scenarios where lobby state must be
/// kept consistent across multiple clients. It supports participant management, team assignments, chat messaging, and
/// game configuration. Only the host can perform certain actions, such as changing settings or starting the game. The
/// class implements IDisposable to ensure proper cleanup of resources, including the gRPC streaming
/// connection.</remarks>
/// <param name="lobbyId">The unique identifier for the lobby, used to reference and manage the lobby in server communications.</param>
/// <param name="stateUpdater">An asynchronous server streaming call that provides real-time updates to the lobby state, enabling synchronization
/// of lobby events between the client and server.</param>
/// <param name="gRPCClient">The gRPC client used to communicate with the lobby service for operations such as sending messages, updating state,
/// and managing lobby membership.</param>
/// <param name="setup">An object containing the initial configuration for the lobby, including the local participant, teams, game settings,
/// and map.</param>
/// <param name="serverAPI">An interface for interacting with the battlegrounds server API, providing methods for server-side operations related
/// to the lobby.</param>
/// <param name="userService">A service for managing user-related operations, such as retrieving the local user's token and information.</param>
/// <param name="companyService">A service for managing company-related operations, such as retrieving and updating company data for participants.</param>
public sealed class MultiplayerLobby(
    string lobbyId, 
    AsyncServerStreamingCall<LobbyStateUpdate> stateUpdater, 
    Proto.Lobbies.LobbyService.LobbyServiceClient gRPCClient, 
    LobbySetup setup,
    IBattlegroundsServerAPI serverAPI,
    IUserService userService,
    ICompanyService companyService,
    IGameMapService mapService) : ILobby, IDisposable {

    private readonly ILogger _logger = Log.ForContext<MultiplayerLobby>();
    private readonly string _lobbyId = lobbyId;

    private readonly AsyncServerStreamingCall<LobbyStateUpdate> _stateUpdater = stateUpdater;
    private readonly Proto.Lobbies.LobbyService.LobbyServiceClient _gRPCClient = gRPCClient;
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI;
    private readonly ICompanyService _companyService = companyService;
    private readonly IUserService _userService = userService;
    private readonly IGameMapService _mapService = mapService;

    private readonly Participant _localParticipant = setup.Self;
    private readonly HashSet<Participant> _participants = setup.Participants;
    private readonly List<LobbySetting> _settings = setup.Settings;
    private readonly Dictionary<string, Company> _companies = [];
    private readonly Channel<LobbyEvent> _internalEvents = Channel.CreateUnbounded<LobbyEvent>();

    private readonly Team _team1 = setup.Team1;
    private readonly Team _team2 = setup.Team2;
    private readonly Team[] _teams = [setup.Team1, setup.Team2];

    private bool _isActive = true;
    private bool _disposedValue = false;

    private Map _map = setup.Map;

    public string Name { get; } = setup.Name;

    public bool IsHost { get; init; } = true; // Assuming the host is the one who created the lobby

    public bool IsActive => _isActive;

    public ISet<Participant> Participants => _participants;

    public Team Team1 => _team1;

    public Team Team2 => _team2;

    public Game Game { get; } = setup.Game;

    public Dictionary<string, Company> Companies => _companies;

    public IList<LobbySetting> Settings => _settings;

    public Map Map => _map;

    public string? GetLocalPlayerId() => _localParticipant.ParticipantId;

    private Metadata GetGrpcMetadata() {
        var token = $"Bearer {_userService.GetLocalUserToken()}";
        return new Metadata {
            { "authorization", token },
            { "x-lobby-id", _lobbyId },
            { "x-participant-id", _localParticipant.ParticipantId }
        };
    }

    private int GetIndexOfTeam(Team? team) {
        if (team == null) {
            return -1;
        }
        if (team == _team1) {
            return 0;
        }
        if (team == _team2) {
            return 1;
        }
        return -1; // Team not found
    } 

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

    public async ValueTask<LobbyEvent?> GetNextEvent() {
        try {
            return await _internalEvents.Reader.ReadAsync();
        } catch (Exception ex) {
            _logger.Error(ex, "Error while getting next lobby event");
            return null;
        }
    }

    /// <summary>
    /// Continuously polls the gRPC stream for lobby updates and forwards them as lobby events to the internal event
    /// channel for UI consumption.
    /// </summary>
    /// <remarks>This method runs as long as the instance remains active, handling lobby updates received from
    /// the gRPC stream. If the stream is cancelled, such as when leaving the lobby or shutting down, polling stops
    /// gracefully. Any unrecognized lobby updates are converted to a default system message event. Errors encountered
    /// during polling are logged for diagnostic purposes.</remarks>
    /// <returns>A task that represents the asynchronous polling operation.</returns>
    public async Task PollGrpcUpdates() {
        // Polls the gRPC stream for lobby updates and pushes them to the internal channel as LobbyEvents for the UI to consume
        // That avoids the issue of reading from the gRPC stream and fetching the next internal event (ie. for client-side actions) at the same time
        while (_isActive) {
            try {
                if (await _stateUpdater.ResponseStream.MoveNext()) {
                    var lobbyEvent = MapAndApplyGrpcEvent(_stateUpdater.ResponseStream.Current);
                    _internalEvents.Writer.TryWrite(lobbyEvent ?? new LobbyEvent(LobbyEventType.SystemMessage, "Received an unrecognized lobby update from the server.")); // Map the gRPC update to a LobbyEvent and push it to the internal channel
                }
            } catch (RpcException rpcEx) when (rpcEx.StatusCode is StatusCode.Cancelled or StatusCode.Unavailable) {
                _logger.Information("gRPC lobby updates stream was cancelled or is unavailable, likely due to leaving the lobby or shutting down. Stopping the update poller.");
                break; // Exit the loop if the stream was cancelled
            } catch (Exception ex) {
                _logger.Error(ex, "Error while polling gRPC lobby updates");
            }
        }
    }

    // Maps a gRPC lobby state update to a LobbyEvent and applies the necessary changes to the local lobby state. If the update type is unrecognized, returns null.
    // The returned type is for triggering UI updates based on the event, as some events may not require a UI update.
    private LobbyEvent? MapAndApplyGrpcEvent(LobbyStateUpdate? update) { 
        if (update == null) {
            return null;
        }
        if (!Enum.TryParse(update.EventType, true, out LobbyEventType eventType)) {
            _logger.Warning("Unknown gRPC lobby event type: {EventType}", update.EventType);
            return null;
        }
        switch (eventType) {
            case LobbyEventType.ParticipantJoined:
                throw new NotImplementedException("Participant joined event is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.ParticipantLeft:
                throw new NotImplementedException("Participant left event is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.ParticipantMessage:
                var senderName = _participants.FirstOrDefault(p => p.ParticipantId == update.ChatMessage.SenderId)?.ParticipantName ?? "Unknown";
                var channel = Enum.TryParse(update.ChatMessage.Channel, true, out ChatChannel parsedChannel) ? parsedChannel : ChatChannel.All;
                return new LobbyEvent(LobbyEventType.ParticipantMessage,
                    new ChatMessage(update.ChatMessage.SenderId, senderName, channel, update.ChatMessage.Content));
            case LobbyEventType.TeamUpdated:
                for (int i = 0; i < update.TeamUpdate.Slots.Count; i++) {
                    var slot = update.TeamUpdate.Slots[i];
                    _teams[update.TeamUpdate.Id].Slots[i] = new Team.Slot(i, slot.ParticipantId, slot.Faction, slot.CompanyId, AIDifficulty.FromName(slot.AiDifficulty), slot.Hidden, slot.Locked);
                }
                return new LobbyEvent(LobbyEventType.TeamUpdated, update.TeamUpdate.Id);
            case LobbyEventType.SlotUpdated:
                var updatedSlot = update.SlotUpdate.Slot;
                _teams[update.SlotUpdate.TeamId].Slots[updatedSlot.Id] = new(updatedSlot.Id, updatedSlot.ParticipantId, updatedSlot.Faction, updatedSlot.CompanyId, AIDifficulty.FromName(updatedSlot.AiDifficulty), updatedSlot.Hidden, updatedSlot.Locked);
                return new LobbyEvent(LobbyEventType.TeamUpdated, update.SlotUpdate.TeamId); // Make UI simply update the whole team when a slot is updated for simplicity, as that's what the UI currently supports
            case LobbyEventType.SettingUpdated:
                var newSetting = update.SettingsUpdate;
                int indexOfSetting = _settings.FindIndex(x => x.Name == newSetting.Key);
                if (indexOfSetting != -1) {
                   var currentSetting = _settings[indexOfSetting];
                    int mappedValue = currentSetting.Type switch {
                        LobbySettingType.Boolean => newSetting.NewValue == "true" ? 1 : 0,
                        LobbySettingType.Integer => int.TryParse(newSetting.NewValue, out var intValue) ? intValue : currentSetting.Value,
                        _ => currentSetting.Value
                    };
                    _settings[indexOfSetting].Value = mappedValue;
                    return new LobbyEvent(LobbyEventType.SettingUpdated, _settings[indexOfSetting]);
                } else {
                    _logger.Warning("Received update for unknown setting: {SettingKey}", newSetting.Key);
                    return null; // Setting not found, ignore the update
                }
            case LobbyEventType.MapUpdated:
                var newMap = _mapService.GetMapByScenarioName(Game, update.SettingsUpdate.NewValue); // Re-use the SettingsUpdate message to get the new map name, as the server doesn't send a separate message for map updates currently
                _map = newMap;
                return new LobbyEvent(LobbyEventType.MapUpdated, newMap);
            case LobbyEventType.GameStarted:
                throw new NotImplementedException($"Event type {eventType} is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.GameCancelled:
                throw new NotImplementedException($"Event type {eventType} is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.GameEnded:
                throw new NotImplementedException($"Event type {eventType} is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.SystemMessage:
            case LobbyEventType.SystemError:
                return new LobbyEvent(eventType, update.SystemMessage.Content);
            default:
                _logger.Warning("Unhandled gRPC lobby event type: {EventType}", eventType);
                break;
        }
        return null; // No event to return
    }

    public Task<LaunchGameResult> LaunchGame() {
        throw new NotImplementedException();
    }

    public Task RemoveAI(Team team, int slotIndex) {
        if (!IsHost) {
            return Task.CompletedTask; // Only the host can remove AI
        }
        throw new NotImplementedException();
    }

    public ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult) {
        if (!IsHost) {
            return ValueTask.FromResult(false); // Only the host can report match results
        }
        throw new NotImplementedException();
    }

    public async Task SendMessage(ChatChannel channel, string msg) {
        var chatMessage = new ChatMessage(_localParticipant.ParticipantId, _localParticipant.ParticipantName, channel, msg);
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, chatMessage));
        await _gRPCClient.SendChatMessageAsync(new Proto.Lobbies.ChatMessage {
            SenderId = chatMessage.SenderId,
            Content = chatMessage.Message,
            Channel = channel.ToString().ToLowerInvariant(),
            LobbyId = _lobbyId
        }, GetGrpcMetadata());
    }

    public async Task SetCompany(Team team, int slotId, string companyId) {
        var local = GetLocalPlayerSlot();
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            ParticipantId = _localParticipant.ParticipantId,
            SlotUpdate = new SlotUpdate {
                TeamId = GetIndexOfTeam(team),
                Slot = new Slot {
                    Id = slotId,
                    ParticipantId = team.Slots[slotId].ParticipantId ?? string.Empty,
                    Faction = team.Slots[slotId].Faction,
                    CompanyId = companyId,
                    AiDifficulty = team.Slots[slotId].Difficulty.ToString(),
                    Hidden = team.Slots[slotId].Hidden,
                    Locked = team.Slots[slotId].Locked
                }
            },
        }, GetGrpcMetadata());
        if (IsHost || (local.team == team && slotId == local.slotId)) {
            // Push the local event too to update the UI immediately
            team.Slots[slotId] = team.Slots[slotId] with { CompanyId = companyId };
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI
        }
    }

    public async Task<bool> SetMap(Map map) {
        if (!IsHost) {
            return false; // Only the host can set the map
        }
        var updateMap = await _gRPCClient.ChangeMapAsync(new() {
            LobbyId = _lobbyId,
            ParticipantId = _localParticipant.ParticipantId,
            NewMap = new() {
                MaxPlayers = map.MaxPlayers,
                MapId = map.ScenarioName
            }
        }, GetGrpcMetadata());
        if (updateMap is null || !updateMap.Success) {
            var errorReason = updateMap?.ErrorReason switch {
                1 => "The specified map was not found on the server.",
                2 => "The map is invalid or corrupted.",
                3 => "Failed to load the map due to a server error.",
                4 => "Map max player count cannot be less than current number of participants",
                _ => "An unknown error occurred while updating the map."
            };
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SystemError, "Failed to update the map. "+ errorReason)); // Notify the UI about the failure and reason
            return false;
        }
        _map = map;
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.MapUpdated, map)); // Notify the UI of map change
        return true;
    }

    public Task SetSetting(LobbySetting newSetting) {
        if (!IsHost) {
            return Task.CompletedTask; // Only the host can set settings
        }
        throw new NotImplementedException();
    }

    public async Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty) {
        if (!IsHost) {
            return; // Only the host can set AI difficulty
        }
        int teamId = GetIndexOfTeam(team);
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            ParticipantId = _localParticipant.ParticipantId,
            SlotUpdate = new SlotUpdate {
                TeamId = teamId,
                Slot = new Slot {
                    Id = slotIndex,
                    ParticipantId = team.Slots[slotIndex].ParticipantId ?? string.Empty,
                    Faction = team.Slots[slotIndex].Faction,
                    CompanyId = team.Slots[slotIndex].CompanyId,
                    AiDifficulty = difficulty.Name,
                    Hidden = team.Slots[slotIndex].Hidden,
                    Locked = team.Slots[slotIndex].Locked
                }
            },
        }, GetGrpcMetadata());
        team.Slots[slotIndex] = team.Slots[slotIndex] with { Difficulty = difficulty }; // Update local state
        _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.TeamUpdated, teamId)); // Notify the UI of the change
    }

    public Task ToggleSlotLock(Team team, int slotIndex) {
        if (!IsHost) {
            return Task.CompletedTask; // Only the host can toggle slot locks
        }
        throw new NotImplementedException();
    }

    public async ValueTask<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) {
        var result = await _serverAPI.UploadGamemodeAsync(_lobbyId, gamemodeLocation);
        if (!result) {
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SystemError, "Failed to upload gamemode. Please report this issue.")); // Notify the UI about the failure
            return new UploadGamemodeResult() { Failed = true };
        }
        return new UploadGamemodeResult() { Failed = false };
    }

    public ValueTask<bool> WaitForAllPlayersHaveGamemode() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Publishes the initial state of the local multiplayer lobby to the server, including team configurations and
    /// lobby settings.
    /// </summary>
    /// <remarks>Call this method during lobby initialization to ensure the server receives the current team
    /// assignments and all lobby settings. This is typically required before players can join or interact with the
    /// lobby.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task PublishInitialState() {

        // Tell server about the local lobby state
        await PublishTeam(0, setup.Team1);
        await PublishTeam(1, setup.Team2);

        foreach (var setting in setup.Settings) {
            await PublishSetting(setting);
        }

    }

    private async Task PublishTeam(int tid, Team team) {
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            ParticipantId = _localParticipant.ParticipantId,
            EventType = LobbyEventType.TeamUpdated.ToString(),
            TeamUpdate = new Proto.Lobbies.Team {
                Id = tid,
                Alias = team.TeamAlias,
                Type = team.TeamType.ToString(),
                Slots = { team.Slots.Select(slot => new Slot {
                    Id = slot.Index,
                    ParticipantId = slot.ParticipantId ?? string.Empty,
                    Faction = slot.Faction,
                    CompanyId = slot.CompanyId,
                    AiDifficulty = slot.Difficulty.ToString(),
                    Hidden = slot.Hidden,
                    Locked = slot.Locked
                }) }
            }
        }, GetGrpcMetadata());
    }

    private async Task PublishSlot(int tid, int slotId, Team.Slot slot) {
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new Proto.Lobbies.SlotUpdate {
                TeamId = tid,
                Slot = new Slot {
                    Id = slotId,
                    ParticipantId = slot.ParticipantId ?? string.Empty,
                    Faction = slot.Faction,
                    CompanyId = slot.CompanyId,
                    AiDifficulty = slot.Difficulty.ToString(),
                    Hidden = slot.Hidden,
                    Locked = slot.Locked
                }
            },
        }, GetGrpcMetadata());
    }

    private async Task PublishSetting(LobbySetting setting) {
        var metadata = GetGrpcMetadata();
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SettingUpdated.ToString(),
            ParticipantId = _localParticipant.ParticipantId,
            SettingsUpdate = new Proto.Lobbies.LobbySetting {
                Key = setting.Name,
                NewValue = setting.Value.ToString(),
            },
        }, metadata);
    }

    public void Dispose() {
        if (!_disposedValue) {
            _isActive = false;
            _disposedValue = true;
            // Close connection with the server (and the lobby) and dispose of the gRPC client
            _stateUpdater.Dispose();
        }
    }

    public async Task LeaveAsync() {
        await _gRPCClient.LeaveLobbyAsync(new LeaveLobbyRequest {
            LobbyId = _lobbyId,
            ParticipantId = _localParticipant.ParticipantId
        }, GetGrpcMetadata());
    }

}
