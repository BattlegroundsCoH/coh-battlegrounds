using Battlegrounds.Core.Lobbies;
using Battlegrounds.Core.Users;
using Battlegrounds.Grpc;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services;

public class LobbyService(
    ILogger<LobbyService> logger, 
    Grpc.LobbyService.LobbyServiceClient client,
    IServiceProvider serviceProvider) : ILobbyService {

    private readonly ILogger<LobbyService> _logger = logger;
    private readonly Grpc.LobbyService.LobbyServiceClient _client = client;

    public async Task<ILobbyService.LobbyDTO[]> GetLobbiesAsync() {
        _logger.LogInformation("Fetching lobbies");

        var result = await _client.GetLobbiesAsync(new GetLobbiesRequest());
        if (result is null) {
            return [];
        }

        return result.Lobbies.Select(x => new ILobbyService.LobbyDTO(Guid.Parse(x.Guid), x.Name, Array.Empty<ILobbyService.LobbyTeamDTO>(), x.IsPasswordProtected, x.Game)).ToArray();

    }

    public async Task<ILobby> HostLobby(UserContext userContext, string name, string game, string password) {

        HostLobbyRequest request = new HostLobbyRequest {
            Name = name,
            Game = game,
            Password = password,
            User = new LobbyUserContext { UserDisplayName = userContext.UserDisplayName, UserId = userContext.UserId, ClientHash = userContext.ClientHash },
        };

        var stream = _client.HostLobby(request);

        if (!await stream.ResponseStream.MoveNext()) {
            return null;
        }

        var response = stream.ResponseStream.Current;
        if (response.ResponseDataCase is not LobbyStatusResponse.ResponseDataOneofCase.Lobby) {
            return null;
        }

        return GRPCLobby.New(serviceProvider, client, stream, response.User);

    }

    public Task<ILobby> JoinLobby(UserContext userContext, Guid guid, string password) {
        throw new NotImplementedException();
    }

}
