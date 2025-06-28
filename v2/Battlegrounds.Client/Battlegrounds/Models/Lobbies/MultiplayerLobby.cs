using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Proto.Lobbies;

using Grpc.Core;

using Serilog;

namespace Battlegrounds.Models.Lobbies;

public sealed class MultiplayerLobby(string lobbyId, AsyncServerStreamingCall<LobbyStateUpdate> stateUpdater, LobbyService.LobbyServiceClient gRPCClient, Participant self) : ILobby, IDisposable {

    private readonly ILogger _logger = Log.ForContext<MultiplayerLobby>();
    private readonly string _lobbyId = lobbyId;

    private readonly AsyncServerStreamingCall<LobbyStateUpdate> _stateUpdater = stateUpdater;
    private readonly LobbyService.LobbyServiceClient _gRPCClient = gRPCClient;

    private readonly Participant _localParticipant = self;
    private readonly HashSet<Participant> _participants = [self];
    private readonly Channel<LobbyEvent> _internalEvents = Channel.CreateUnbounded<LobbyEvent>();

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

    private bool _isActive = true;

    public string Name { get; init; } = string.Empty;

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
        throw new NotImplementedException("Mapping gRPC LobbyStateUpdate to LobbyEvent is not implemented yet.");
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

    public static async Task<MultiplayerLobby> ForGrpcObjects(LobbyService.LobbyServiceClient client, User localUser, HostLobbyRequest hostRequest, Configuration configuration) {

        var listenStream = client.HostLobby(hostRequest);
        var localParticipant = new Participant(0, localUser.UserId, localUser.UserDisplayName, false, true);

        if (!await listenStream.ResponseStream.MoveNext()) {
            throw new InvalidOperationException("Failed to start lobby. No response received from server.");
        }

        // Await for the first response to get the lobby ID
        var hostResponse = listenStream.ResponseStream.Current;

        return new MultiplayerLobby(hostResponse.LobbyId, listenStream, client, localParticipant) {
            Name = hostRequest.LobbyName,
            IsHost = true, // The host is the one who created the lobby
        };

    }

    public void Dispose() {
        throw new NotImplementedException();
    }

}
