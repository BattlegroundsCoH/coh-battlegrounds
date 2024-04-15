using Battlegrounds.Grpc;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services;

public class LobbyService(
    ILogger<LobbyService> logger, 
    Grpc.LobbyService.LobbyServiceClient client) : ILobbyService {

    private readonly ILogger<LobbyService> _logger = logger;
    private readonly Grpc.LobbyService.LobbyServiceClient _client = client;

    public async Task<ILobbyService.LobbyDTO[]> GetLobbiesAsync() {
        _logger.LogInformation("Fetching lobbies");

        var result = await _client.GetLobbiesAsync(new GetLobbiesRequest());
        if (result is null) {
            return [];
        }

        return result.Lobbies.Select(x => new ILobbyService.LobbyDTO()).ToArray();

    }

}
