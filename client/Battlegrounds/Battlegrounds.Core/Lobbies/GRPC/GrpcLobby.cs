using Battlegrounds.Core.Games.Scenarios;
using Battlegrounds.Grpc;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobby(
    LobbyService.LobbyServiceClient client,
    AsyncServerStreamingCall<LobbyStatusResponse> stream,
    LobbyUserContext userContext,
    ILogger<GrpcLobby> logger) : ILobby {

    private readonly ILogger<GrpcLobby> _logger = logger;
    private readonly LobbyService.LobbyServiceClient _client = client;
    private readonly AsyncServerStreamingCall<LobbyStatusResponse> _stream = stream;
    private readonly LobbyUserContext _userContext = userContext;

    private readonly GrpcLobbyTeam _team1 = new();
    private readonly GrpcLobbyTeam _team2 = new();
    private readonly Dictionary<string, string> _settings = [];

    private Task _listenTask = Task.CompletedTask;
    private bool _isHost;
    private Action? _onUpdate;
    private Action<ILobbyChatMessage>? _onChatMessage;

    public Guid Guid { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Game { get; init; } = string.Empty;

    public bool IsHost => _isHost;

    public ILobbyTeam Team1 => _team1;

    public ILobbyTeam Team2 => _team2;

    public IDictionary<string, string> Settings => _settings;

    public ulong LocalPlayerId => _userContext.UserId;

    private async Task ListenChanges() {
        _logger.LogInformation("Started listening for incoming lobby messages");
        try {
            await foreach (var next in _stream.ResponseStream.ReadAllAsync()) {
                switch (next.ResponseDataCase) {
                    case LobbyStatusResponse.ResponseDataOneofCase.Message:
                        break;
                    case LobbyStatusResponse.ResponseDataOneofCase.Lobby:
                        FromProto(next.Lobby);
                        break;
                    default:
                        break;
                }
            }
        } catch (RpcException rex) { 
            if (rex.StatusCode != StatusCode.Cancelled) {
                _logger.LogError(rex, "Unexpected gRPC exception occured while executing gRPC listener");
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected exception occured while executing gRPC listener");
        }
        _logger.LogInformation("Stopped listening for incoming lobby messages");
    }

    public void SetUpdateCallback(Action callback) => _onUpdate = callback;

    public void SetChatCallback(Action<ILobbyChatMessage> callback) => _onChatMessage = callback;

    private void FromProto(Lobby lobby, bool triggerUpdate = true) {

        _isHost = lobby.HostId == _userContext.UserId;

        _team1.FromProto(lobby.TeamA);
        _team2.FromProto(lobby.TeamB);

        _settings.Clear();
        foreach (var (k,v) in lobby.Settings) {
            _settings.Add(k, v);
        }

        if (triggerUpdate) {
            _onUpdate?.Invoke();
        }

    }

    public async Task SetTeamNames(string team1, string team2) {
        await _client.UpdateTeamAsync(new UpdateTeamRequest { User = _userContext, Alliance = _team1.Alliance, Name = team1, Team = 0 });
        await _client.UpdateTeamAsync(new UpdateTeamRequest { User = _userContext, Alliance = _team2.Alliance, Name = team2, Team = 1 });
    }

    public async Task MoveToSlot(int team, int slot) {
        var request = new ChangeSlotRequest { User = _userContext, Slot = slot, Team = team };
        await _client.ChangeSlotAsync(request);
    }

    public async Task SetDifficulty(int team, int slot, int aiDifficulty) {
        var slotValue = TeamFromIndex(team).Slots[slot];
        var request = new UpdateSlotRequest { User = _userContext, Team = team, Slot = slot, State = MapToProtoSlotState(slotValue), AiLevel = MapToProtoAIDifficulty(aiDifficulty), UserCompany = "" };
        await _client.UpdateSlotAsync(request);
    }

    public async Task SetCompany(int team, int slot, string company) {
        var slotValue = TeamFromIndex(team).Slots[slot];
        var request = new UpdateSlotRequest { User = _userContext, Team = team, Slot = slot, State = MapToProtoSlotState(slotValue), AiLevel = MapToProtoAIDifficulty(-1), UserCompany = company };
        await _client.UpdateSlotAsync(request);
    }

    public async Task SetLocked(int team, int slot, bool isLocked) {
        var request = new UpdateSlotRequest { User = _userContext, Team = team, Slot = slot, State = isLocked ? LobbySlotState.StateLocked : LobbySlotState.StateUnlocked, AiLevel = LobbySlotAIState.AiHuman, UserCompany = "" };
        await _client.UpdateSlotAsync(request);
    }

    public async Task SetSetting(string setting, string value) {
        var request = new UpdateSettingsRequest { User = _userContext, Override = false, Settings = { [setting] = value } };
        var response = await _client.UpdateSettingsAsync(request);
        if (request.Override) {
            _settings.Clear();
            foreach (var (k,v) in response.Settings) {
                _settings[k] = v;
            }
        } else {
            _settings[setting] = response.Settings[setting];
        }
    }

    public async Task SetScenario(IScenario scenario) {
        throw new NotImplementedException();
    }

    public async Task LaunchMatch() {
        throw new NotImplementedException();
    }

    public async Task Leave() {
        var request = new LeaveRequest { User = _userContext };
        await _client.LeaveAsync(request);
        _stream.Dispose();
    }

    private LobbySlotAIState MapToProtoAIDifficulty(int level) => level switch {
        0 => LobbySlotAIState.AiHuman,
        1 => LobbySlotAIState.AiEasy,
        2 => LobbySlotAIState.AiNormal,
        3 => LobbySlotAIState.AiHard,
        4 => LobbySlotAIState.AiExpert,
        _ => throw new ArgumentException($"Invalid AI level {level}")
    };

    private LobbySlotState MapToProtoSlotState(ILobbySlot slot) {
        if (!slot.IsVisible) {
            return LobbySlotState.StateUnavailable;
        }
        if (slot.IsLocked) {
            return LobbySlotState.StateLocked;
        }
        return slot.Player == null ? LobbySlotState.StateOccupied : LobbySlotState.StateUnlocked;
    }

    private GrpcLobbyTeam TeamFromIndex(int index) => index == 0 ? _team1 : _team2;

    public static GrpcLobby New(IServiceProvider serviceProvider, Lobby lobby, LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStatusResponse> stream, LobbyUserContext userContext, bool isHost) {

        GrpcLobby gRPCLobby = new(client, stream, userContext, serviceProvider.GetService(typeof(ILogger<GrpcLobby>)) as ILogger<GrpcLobby> ?? throw new Exception()) {
            Guid = Guid.Parse(userContext.Guid),
            Name = lobby.Name,
            Game = lobby.Game,
            _isHost = isHost
        };

        gRPCLobby.FromProto(lobby, false);
        gRPCLobby._listenTask = Task.Run(gRPCLobby.ListenChanges);

        return gRPCLobby;

    }

}
