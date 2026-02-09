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

public sealed class MultiplayerLobby(
    string lobbyId, 
    AsyncServerStreamingCall<LobbyStateUpdate> stateUpdater, 
    Proto.Lobbies.LobbyService.LobbyServiceClient gRPCClient, 
    LobbySetup setup,
    IBattlegroundsServerAPI serverAPI,
    IUserService userService,
    ICompanyService companyService) : ILobby, IDisposable {

    private readonly ILogger _logger = Log.ForContext<MultiplayerLobby>();
    private readonly string _lobbyId = lobbyId;

    private readonly AsyncServerStreamingCall<LobbyStateUpdate> _stateUpdater = stateUpdater;
    private readonly Proto.Lobbies.LobbyService.LobbyServiceClient _gRPCClient = gRPCClient;
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI;
    private readonly ICompanyService _companyService = companyService;
    private readonly IUserService _userService = userService;

    private readonly Participant _localParticipant = setup.Self;
    private readonly HashSet<Participant> _participants = [setup.Self];
    private readonly List<LobbySetting> _settings = setup.Settings;
    private readonly Dictionary<string, Company> _companies = [];
    private readonly Channel<LobbyEvent> _internalEvents = Channel.CreateUnbounded<LobbyEvent>();

    private readonly Team _team1 = setup.Team1;
    private readonly Team _team2 = setup.Team2;

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

    public async Task PollGrpcUpdates() {
        // Polls the gRPC stream for lobby updates and pushes them to the internal channel as LobbyEvents for the UI to consume
        // That avoids the issue of reading from the gRPC stream and fetching the next internal event (ie. for client-side actions) at the same time
        while (_isActive) {
            try {
                if (await _stateUpdater.ResponseStream.MoveNext()) {
                    var lobbyEvent = MapGrpcLobbyStateToLobbyEvent(_stateUpdater.ResponseStream.Current);
                    _internalEvents.Writer.TryWrite(lobbyEvent ?? new LobbyEvent(LobbyEventType.SystemMessage, "Received an unrecognized lobby update from the server.")); // Map the gRPC update to a LobbyEvent and push it to the internal channel
                }
            } catch (RpcException rpcEx) when (rpcEx.StatusCode is StatusCode.Cancelled) {
                _logger.Information("gRPC lobby updates stream was cancelled, likely due to leaving the lobby or shutting down. Stopping the update poller.");
                break; // Exit the loop if the stream was cancelled
            } catch (Exception ex) {
                _logger.Error(ex, "Error while polling gRPC lobby updates");
            }
        }
    }

    private LobbyEvent? MapGrpcLobbyStateToLobbyEvent(LobbyStateUpdate? update) { 
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
            case LobbyEventType.TeamUpdated:
                
                throw new NotImplementedException("Team updates are not yet implemented in the gRPC lobby handler.");
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
                    var updateSettingEvent = new LobbyEvent(LobbyEventType.SettingUpdated, new LobbySetting { Name = newSetting.Key, Type = currentSetting.Type, Value = mappedValue });
                    return updateSettingEvent;
                } else {
                    _logger.Warning("Received update for unknown setting: {SettingKey}", newSetting.Key);
                    return null; // Setting not found, ignore the update
                }
            case LobbyEventType.MapUpdated:
                throw new NotImplementedException("Map updates are not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.GameStarted:
            case LobbyEventType.GameCancelled:
            case LobbyEventType.GameEnded:
            case LobbyEventType.SystemMessage:
            case LobbyEventType.SystemError:
                throw new NotImplementedException($"Event type {eventType} is not yet implemented in the gRPC lobby handler.");
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

    public Task<bool> SetMap(Map map) {
        if (!IsHost) {
            return Task.FromResult(false); // Only the host can set the map
        }
        throw new NotImplementedException();
    }

    public Task SetSetting(LobbySetting newSetting) {
        if (!IsHost) {
            return Task.CompletedTask; // Only the host can set settings
        }
        throw new NotImplementedException();
    }

    public Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty) {
        if (!IsHost) {
            return Task.CompletedTask; // Only the host can set AI difficulty
        }
        throw new NotImplementedException();
    }

    public Task ToggleSlotLock(Team team, int slotIndex) {
        if (!IsHost) {
            return Task.CompletedTask; // Only the host can toggle slot locks
        }
        throw new NotImplementedException();
    }

    public Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) {
        throw new NotImplementedException();
    }

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
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SettingUpdated.ToString(),
            SettingsUpdate = new Proto.Lobbies.LobbySetting {
                Key = setting.Name,
                NewValue = setting.Value.ToString(),
            },
        }, GetGrpcMetadata());
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
