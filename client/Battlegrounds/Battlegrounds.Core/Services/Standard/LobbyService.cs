using Battlegrounds.Core.Games.Scenarios;
using Battlegrounds.Core.Lobbies;
using Battlegrounds.Core.Lobbies.GRPC;
using Battlegrounds.Core.Users;
using Battlegrounds.Grpc;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services.Standard;

public class LobbyService(
    ILogger<LobbyService> logger,
    Grpc.LobbyService.LobbyServiceClient client,
    IServiceProvider serviceProvider) : ILobbyService {

    private readonly ILogger<LobbyService> _logger = logger;
    private readonly Grpc.LobbyService.LobbyServiceClient _client = client;
    private readonly Dictionary<Guid, ILobby> _activeLobbies = [];

    public ILobby? GetActiveLobby(Guid guid) 
        => _activeLobbies.TryGetValue(guid, out var activeLobby) ? activeLobby : null;

    public async Task<ILobbyService.LobbyDTO[]> GetLobbiesAsync() {
        _logger.LogInformation("Fetching lobbies");

        var result = await _client.GetLobbiesAsync(new GetLobbiesRequest());
        if (result is null) {
            return [];
        }

        return result.Lobbies.Select(x => new ILobbyService.LobbyDTO(Guid.Parse(x.Guid), x.Name, [], x.IsPasswordProtected, x.Game)).ToArray();

    }

    public async Task<ILobby?> HostLobby(UserContext userContext, string name, string game, string password, IScenario scenario, IDictionary<string, string> settings) {

        HostLobbyRequest request = new HostLobbyRequest {
            Name = name,
            Game = game,
            Password = password,
            User = new LobbyUserContext { UserDisplayName = userContext.UserDisplayName, UserId = userContext.UserId, ClientHash = userContext.ClientHash },
            Scenario = scenario.AsProto(),
        };

        request.Settings.Add(settings);

        var stream = _client.HostLobby(request);

        if (!await stream.ResponseStream.MoveNext()) {
            return null;
        }

        var response = stream.ResponseStream.Current;
        if (response.ResponseDataCase is not LobbyStatusResponse.ResponseDataOneofCase.Lobby) {
            return null;
        }

        var lobby = GrpcLobby.New(serviceProvider, response.Lobby, _client, stream, response.User, isHost: true);
        _activeLobbies[Guid.Parse(response.User.Guid)] = lobby;

        return lobby;

    }

    public Task<ILobby?> JoinLobby(UserContext userContext, Guid guid, string password) {
        throw new NotImplementedException();
    }

    public void RemoveActiveLobby(Guid guid) {
        _activeLobbies.Remove(guid);
    }

}
