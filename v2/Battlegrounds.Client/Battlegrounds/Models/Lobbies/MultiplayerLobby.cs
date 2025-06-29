using System.Threading.Channels;

using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Services;

using Grpc.Core;

using Serilog;

namespace Battlegrounds.Models.Lobbies;

public sealed class MultiplayerLobby(
    string lobbyId, 
    AsyncServerStreamingCall<LobbyStateUpdate> stateUpdater, 
    Proto.Lobbies.LobbyService.LobbyServiceClient gRPCClient, 
    LobbySetup setup,
    IBattlegroundsServerAPI serverAPI,
    ICompanyService companyService) : ILobby, IDisposable {

    private readonly ILogger _logger = Log.ForContext<MultiplayerLobby>();
    private readonly string _lobbyId = lobbyId;

    private readonly AsyncServerStreamingCall<LobbyStateUpdate> _stateUpdater = stateUpdater;
    private readonly Proto.Lobbies.LobbyService.LobbyServiceClient _gRPCClient = gRPCClient;
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI;
    private readonly ICompanyService _companyService = companyService;

    private readonly Participant _localParticipant = setup.Self;
    private readonly HashSet<Participant> _participants = [setup.Self];
    private readonly Channel<LobbyEvent> _internalEvents = Channel.CreateUnbounded<LobbyEvent>();

    private readonly Team _team1 = setup.Team1;
    private readonly Team _team2 = setup.Team2;

    private bool _isActive = true;

    public string Name { get; } = setup.Name;

    public bool IsHost { get; init; } = true; // Assuming the host is the one who created the lobby

    public bool IsActive => _isActive;

    public ISet<Participant> Participants => _participants;

    public Team Team1 => _team1;

    public Team Team2 => _team2;

    public Game Game => throw new NotImplementedException();

    public Dictionary<string, Company> Companies => throw new NotImplementedException();

    public IList<LobbySetting> Settings => throw new NotImplementedException();

    public Map Map => throw new NotImplementedException();

    public string? GetLocalPlayerId() => _localParticipant.ParticipantId;

    public (Team? team, int slotId) GetLocalPlayerSlot() {
        throw new NotImplementedException();
    }

    public async ValueTask<LobbyEvent?> GetNextEvent() {
        try {
            var grpcTask = ReadNextGrpcUpdateAsync();
            var internalTask = _internalEvents.Reader.ReadAsync().AsTask();
            var completedTask = await Task.WhenAny(grpcTask, internalTask);
            if (completedTask == grpcTask) {
                var grpcUpdate = await grpcTask;
                // TODO: Map the gRPC update to a LobbyEvent
                return MapGrpcLobbyStateToLobbyEvent(grpcUpdate);
            } else if (completedTask == internalTask) {
                // Read from the internal channel
                return await internalTask;
            }
            return null; // No event available
        } catch (Exception ex) {
            _logger.Error(ex, "Error while getting next lobby event");
            return null;
        }
    }

    private async Task<LobbyStateUpdate?> ReadNextGrpcUpdateAsync() {
        if (await _stateUpdater.ResponseStream.MoveNext()) {
            return _stateUpdater.ResponseStream.Current;
        } else {
            return null;
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
                throw new NotImplementedException("Setting updates are not yet implemented in the gRPC lobby handler.");
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
        throw new NotImplementedException();
    }

    public ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult) {
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
        });
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

    public Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) {
        throw new NotImplementedException();
    }

    public static async Task<MultiplayerLobby> ForGrpcObjects(
        Proto.Lobbies.LobbyService.LobbyServiceClient client, 
        AsyncServerStreamingCall<LobbyStateUpdate> stream, LobbySetup setup, IBattlegroundsServerAPI serverAPI, ICompanyService companyService) {

        if (!await stream.ResponseStream.MoveNext()) {
            throw new InvalidOperationException("Failed to start lobby. No response received from server.");
        }

        // Await for the first response to get the lobby ID
        var hostResponse = stream.ResponseStream.Current;

        var lobby = new MultiplayerLobby(hostResponse.LobbyId, stream, client, setup, serverAPI, companyService) {
            IsHost = true, // The host is the one who created the lobby
        };

        // Tell server about the local lobby state
        await lobby.PublishTeam(0, setup.Team1);
        await lobby.PublishTeam(1, setup.Team2);

        return lobby;

    }

    private async Task PublishTeam(int tid, Team team) {
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
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
        });
    }

    public void Dispose() {
        throw new NotImplementedException();
    }

    public async Task LeaveAsync() {
        throw new NotImplementedException();
    }

}
