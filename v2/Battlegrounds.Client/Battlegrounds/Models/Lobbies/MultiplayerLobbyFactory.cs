using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Services;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Provides functionality for creating and managing multiplayer lobbies using application services.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It is designed to facilitate the creation of
/// multiplayer lobbies by utilizing services such as server API, company service, and user service. Ensure that the
/// provided service provider is properly configured to access these services.</remarks>
/// <param name="serviceProvider">The service provider used to resolve dependencies required for lobby creation and management. Must be configured to
/// provide all necessary services.</param>
public sealed class MultiplayerLobbyFactory(IServiceProvider serviceProvider) {

    /// <summary>
    /// Asynchronously retrieves and initializes a multiplayer lobby using the specified client, streaming updates, and
    /// lobby setup configuration.
    /// </summary>
    /// <remarks>Ensure that the provided client and streaming call are valid and properly initialized before
    /// invoking this method. The method waits for the first response from the server to obtain the lobby identifier and
    /// establish the lobby context.</remarks>
    /// <param name="client">The gRPC client used to communicate with the lobby service.</param>
    /// <param name="stream">The server streaming call that provides real-time updates about the lobby state.</param>
    /// <param name="setup">The configuration object that defines the initial parameters and settings for the lobby.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created and initialized
    /// MultiplayerLobby instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the initial response from the server is not received, indicating that the lobby could not be started.</exception>
    public async Task<MultiplayerLobby> GetLobby(Proto.Lobbies.LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStateUpdate> stream, LobbySetup setup) {
        var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        var serverAPI = provider.GetRequiredService<IBattlegroundsServerAPI>();
        var companyService = provider.GetRequiredService<ICompanyService>();
        var userService = provider.GetRequiredService<IUserService>();
        var mapService = provider.GetRequiredService<IGameMapService>();

        if (!await stream.ResponseStream.MoveNext()) {
            throw new InvalidOperationException("Failed to start lobby. No response received from server.");
        }

        // Await for the first response to get the lobby ID
        var hostResponse = stream.ResponseStream.Current;

        var lobby = new MultiplayerLobby(hostResponse.LobbyId, stream, client, setup, serverAPI, userService, companyService, mapService) {
            IsHost = true, // The host is the one who created the lobby
        };

        return lobby;

    }

}
