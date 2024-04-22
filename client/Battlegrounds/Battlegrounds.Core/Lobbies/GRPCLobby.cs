
using Battlegrounds.Grpc;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Lobbies;

public sealed class GRPCLobby(
    LobbyService.LobbyServiceClient client, 
    AsyncServerStreamingCall<LobbyStatusResponse> stream, 
    LobbyUserContext userContext,
    ILogger<GRPCLobby> logger) : ILobby {

    private readonly ILogger<GRPCLobby> _logger = logger;
    private readonly LobbyService.LobbyServiceClient _client = client;
    private readonly AsyncServerStreamingCall<LobbyStatusResponse> _stream = stream;
    private readonly LobbyUserContext _userContext = userContext;
    private Task _listenTask = Task.CompletedTask;

    public Guid Guid { get; init; }

    public string Name { get; init; } = string.Empty;

    private async Task ListenChanges() {
        _logger.LogInformation("Started listening for incoming lobby message");
        await foreach (var next in _stream.ResponseStream.ReadAllAsync()) {
            switch (next.ResponseDataCase) {
                case LobbyStatusResponse.ResponseDataOneofCase.Message:
                    break;
                case LobbyStatusResponse.ResponseDataOneofCase.Lobby:
                    break;
                default:
                    break;
            }
        }
    }

    public static GRPCLobby New(IServiceProvider serviceProvider, Lobby lobby, LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStatusResponse> stream, LobbyUserContext userContext) { 

        GRPCLobby gRPCLobby = new (client, stream, userContext, serviceProvider.GetService(typeof(ILogger<GRPCLobby>)) as ILogger<GRPCLobby> ?? throw new Exception()) { 
            Guid = Guid.Parse(userContext.Guid),
            Name = lobby.Name,
        };

        gRPCLobby._listenTask = Task.Run(gRPCLobby.ListenChanges);

        return gRPCLobby;
    }

}
