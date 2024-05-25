using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Games.Scenarios;
using Battlegrounds.Core.Lobbies.Standard;
using Battlegrounds.Core.Services;
using Battlegrounds.Grpc;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobby(
    LobbyService.LobbyServiceClient client,
    AsyncServerStreamingCall<LobbyStatusResponse> stream,
    LobbyUserContext userContext,
    ICompanyService companyService,
    IMatchModBuilderService matchModBuilderService,
    GrpcLobbyResources grpcLobbyResources,
    ILogger<GrpcLobby> logger) : ILobby {

    private readonly ILogger<GrpcLobby> _logger = logger;
    private readonly LobbyService.LobbyServiceClient _client = client;
    private readonly AsyncServerStreamingCall<LobbyStatusResponse> _stream = stream;
    private readonly LobbyUserContext _userContext = userContext;
    private readonly ICompanyService _companyService = companyService;
    private readonly IMatchModBuilderService _matchModBuilderService = matchModBuilderService;
    private readonly GrpcLobbyResources _resources = grpcLobbyResources;

    private readonly GrpcLobbyTeam _team1 = new();
    private readonly GrpcLobbyTeam _team2 = new();
    private readonly Dictionary<string, string> _settings = [];
    private readonly HashSet<ulong> _readyPlayers = [];

    private IScenario _scenario = new GrpcLobbyScenario(new LobbyScenario());
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

    public ISet<ulong> ReadyPlayers => _readyPlayers;

    public IScenario Scenario => _scenario;

    private async Task ListenChanges() {
        _logger.LogInformation("Started listening for incoming lobby messages");
        try {
            await foreach (var next in _stream.ResponseStream.ReadAllAsync()) {
                switch (next.EventType) {
                    case LobbyStatusEvent.StatusCompileGamemode:
                        _ = Task.Run(_compileGamemode);
                        break;
                    default:
                        break;
                }
                switch (next.ResponseDataCase) {
                    case LobbyStatusResponse.ResponseDataOneofCase.Message:
                        PublishMessage(next.Message);
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

    private async Task _compileGamemode() {

        // Try compile
        if (!await _matchModBuilderService.BuildMatchGamemode(this)) {
            _logger.LogCritical("Failed compiling gamemode");
            // TODO: Somehow notify server?
            return;
        }

        // Open the stream
        var gamemode = await _matchModBuilderService.OpenReadGamemodeArchive(Guid);
        if (gamemode is null) {
            _logger.LogCritical("Failed reading gamemode file");
            // TODO: Somehow notify server?
            return;
        }

        // Upload it
        if (!await _resources.UploadResourceAsync(_userContext, "gamemode", LobbyResourceKind.ResourceKindGamemode, gamemode)) {
            _logger.LogCritical("Failed reading uploading file");
            // TODO: Somehow notify server?
            return;
        }

    }

    private void PublishMessage(LobbyChatMessage chatMessage) {
        var timestamp = DateTime.TryParse(chatMessage.Timestamp, out var st) ? st : DateTime.Now;
        ILobbyChatMessage msg = chatMessage.SenderId switch {
            0 => new SystemMessage(timestamp, chatMessage.Message),
            _ => new ChatMessage(timestamp, TryFindUsername(chatMessage.SenderId) ?? chatMessage.SenderId.ToString(), chatMessage.Message)
        };
        _onChatMessage?.Invoke(msg);
    }

    private string? TryFindUsername(ulong userId) {
      
        // Find slot with player
        var slot =
            _team1.Slots.FirstOrDefault(x => x.Player is not null && x.Player.PlayerId == userId) ??
            _team2.Slots.FirstOrDefault(x => x.Player is not null && x.Player.PlayerId == userId);

        if (slot is not null && slot.Player is not null) {
            return slot.Player.Name;
        }
        
        return null;

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

        _scenario = new GrpcLobbyScenario(lobby.Scenario);

        if (triggerUpdate) {
            _onUpdate?.Invoke();
        }

    }

    public async Task SetTeamNames(string team1, string team2) {
        await _client.UpdateTeamAsync(new UpdateTeamRequest { 
            User = _userContext, 
            Team1Alliance = _team1.Alliance, 
            Team1Name = team1, 
            Team2Alliance = _team2.Alliance, 
            Team2Name = team2 
        });
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
        var request = new UpdateScenarioRequest { User = _userContext, Scenario = scenario.AsProto() };
        await _client.UpdateScenarioAsync(request);
    }

    public async Task SetState(bool ready) {
        var request = new SetPlayerStateRequest { User = _userContext, IsReady = ready };
        await _client.SetPlayerStateAsync(request);
    }

    public async Task<bool> UploadCompanyAsync(ICompany company) {
        using var memoryStream = new MemoryStream();
        if (!await _companyService.SaveCompany(company, memoryStream)) {
            return false;
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        return await _resources.UploadResourceAsync(_userContext, company.Id.ToString(), LobbyResourceKind.ResourceKindCompany, memoryStream);
    }

    public Task<bool> UploadGamemodeAsync(Stream inputStream) => _resources.UploadResourceAsync(_userContext, "gamemode", LobbyResourceKind.ResourceKindGamemode, inputStream);

    public async Task<bool> LaunchMatchAsync() {
        var request = new StartPlayRequest { User = _userContext };
        var response = await _client.StartPlayAsync(request);
        if (response != null) {
            return response.IsStarting;
        }
        return false;
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

    public Task<IDictionary<ulong, ICompany>> DownloadCompaniesAsync() {
        throw new NotImplementedException();
    }

    public static GrpcLobby New(IServiceProvider serviceProvider, Lobby lobby, LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStatusResponse> stream, LobbyUserContext userContext, bool isHost) {

        var companyService = serviceProvider.GetService<ICompanyService>() ?? throw new Exception("No company service found!");
        var matchModBuilderService = serviceProvider.GetService<IMatchModBuilderService>() ?? throw new Exception("No match builder service found!");
        var lobbyResourcesLogger = serviceProvider.GetService<ILogger<GrpcLobbyResources>>() ?? throw new Exception("No logger for gRPC resources class found!");
        var logger = serviceProvider.GetService<ILogger<GrpcLobby>>() ?? throw new Exception("No logger for gRPC lobby found!");
        GrpcLobby gRPCLobby = new(client, stream, userContext, companyService, matchModBuilderService, new GrpcLobbyResources(client, lobbyResourcesLogger), logger) {
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
