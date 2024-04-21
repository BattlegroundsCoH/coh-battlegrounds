
using Battlegrounds.Grpc;

using Grpc.Core;

namespace Battlegrounds.Core.Lobbies;

public sealed class GRPCLobby : ILobby {
    
    public Guid Guid { get; init; }

    public string Name => throw new NotImplementedException();

    public static GRPCLobby New(IServiceProvider serviceProvider, LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStatusResponse> stream, LobbyUserContext userContext) { 
        return new GRPCLobby() { Guid = Guid.Parse(userContext.Guid) }; 
    }

}
