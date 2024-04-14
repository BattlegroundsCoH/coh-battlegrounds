using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Grpc;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services;

public class LobbyService : ILobbyService {

    private readonly ILogger<LobbyService> _logger;
    private readonly Grpc.LobbyService.LobbyServiceClient _client;

    public LobbyService(ILogger<LobbyService> logger) { 
        _logger = logger;
    }

    public async Task<ILobbyService.LobbyDTO[]> GetLobbiesAsync() {
        _logger.LogInformation("Fetching lobbies");

        var result = await _client.GetLobbiesAsync(new GetLobbiesRequest());
        if (result is null) {
            return [];
        }

        return result.Lobbies.Select(x => new ILobbyService.LobbyDTO()).ToArray();

    }

}
