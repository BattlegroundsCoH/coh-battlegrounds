using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Models;

using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Factories;

/// <summary>
/// Provides functionality to create gRPC clients for connecting to the Battlegrounds server.
/// </summary>
/// <remarks>This factory is responsible for creating instances of <see cref="LobbyService.LobbyServiceClient"/> 
/// configured to communicate with the Battlegrounds server. The server address is constructed using  the host and port
/// specified in the provided <see cref="Configuration"/> object.</remarks>
/// <param name="logger"></param>
public sealed class GrpcServerClientFactory(ILogger<GrpcServerClientFactory> logger) {

    private readonly ILogger<GrpcServerClientFactory> _logger = logger;

    /// <summary>
    /// Creates and returns a new gRPC client for the Battlegrounds server.
    /// </summary>
    /// <remarks>This method uses the host and port specified in the <paramref name="configuration"/> to
    /// construct the gRPC client. Ensure that the configuration contains valid values for
    /// <c>BattlegroundsServerHost</c> and <c>BattlegroundsGrpcServerPort</c>.</remarks>
    /// <param name="configuration">The configuration containing the host and port information for the Battlegrounds server.</param>
    /// <returns>A <see cref="LobbyService.LobbyServiceClient"/> instance connected to the specified Battlegrounds server.</returns>
    public LobbyService.LobbyServiceClient CreateClient(Configuration configuration) {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        string address = $"{configuration.BattlegroundsServerHost}:{configuration.BattlegroundsGrpcServerPort}";
        _logger.LogInformation("Creating gRPC client for Battlegrounds server at {Address}", address);

        try {

            var channel = GrpcChannel.ForAddress(address);
            var client = new LobbyService.LobbyServiceClient(channel);

            _logger.LogInformation("Successfully created gRPC client for Battlegrounds server at {Address}", address);

            return client;

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to create gRPC client for Battlegrounds server at {Address}", address);
            throw;
        }

    }

}
