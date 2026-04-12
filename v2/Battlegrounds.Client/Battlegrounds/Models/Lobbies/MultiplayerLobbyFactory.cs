using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models.Playing;
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
    public async Task<MultiplayerLobby> GetLobbyAsHost(LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStateUpdate> stream, LobbySetup setup) {
        var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        var serverAPI = provider.GetRequiredService<IBattlegroundsServerAPI>();
        var companyService = provider.GetRequiredService<ICompanyService>();
        var userService = provider.GetRequiredService<IUserService>();
        var mapService = provider.GetRequiredService<IGameMapService>();

        if (!await stream.ResponseStream.MoveNext()) {
            throw new InvalidOperationException("Failed to host lobby. No response received from server.");
        }

        // Await for the first response to get the lobby ID
        var hostResponse = stream.ResponseStream.Current;

        var lobby = new MultiplayerLobby(hostResponse.LobbyId, stream, client, setup, serverAPI, userService, companyService, mapService) {
            IsHost = true,
            IsReady = true // The host is always considered ready
        };

        return lobby;

    }

    /// <summary>
    /// Asynchronously joins an existing multiplayer lobby as a non-host participant using the specified lobby ID and
    /// gRPC client.
    /// </summary>
    /// <param name="lobby">The lobby object representing the lobby to join. Cannot be null.</param>
    /// <param name="client">The gRPC client used to communicate with the lobby service.</param>
    /// <param name="stream">The server streaming call providing lobby state updates. Must be an active stream associated with the specified
    /// lobby.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a configured instance of the
    /// multiplayer lobby for non-host usage.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no response is received from the server when attempting to join the lobby.</exception>
    public async Task<MultiplayerLobby> GetLobbyAsNonHost(BrowserLobby browserLobby, LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStateUpdate> stream) {
        var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        var serverAPI = provider.GetRequiredService<IBattlegroundsServerAPI>();
        var companyService = provider.GetRequiredService<ICompanyService>();
        var userService = provider.GetRequiredService<IUserService>();
        var mapService = provider.GetRequiredService<IGameMapService>();
        var gameService = provider.GetRequiredService<IGameService>();

        if (!await stream.ResponseStream.MoveNext()) {
            throw new InvalidOperationException("Failed to join lobby. No response received from server.");
        }

        // Await for the first response to get the lobby ID
        var joinResponse = stream.ResponseStream.Current;
        if (joinResponse.LobbyState is null) {
            throw new InvalidOperationException("Failed to join lobby. No lobby state received from server.");
        }

        // Construct the initial lobby setup based on the received lobby state
        var selfUser = await userService.GetLocalUserAsync() ?? throw new InvalidOperationException("Failed to retrieve local user information.");
        var game = gameService.GetGame(browserLobby.Game);
        var currentState = joinResponse.LobbyState;
        var setup = new LobbySetup {
            Name = currentState.Name,
            Self = MapProtoParticipant(currentState.Participants.FirstOrDefault(p => p.ParticipantId == selfUser.UserId) ?? throw new InvalidOperationException("Failed to identify self participant in lobby state.")),
            Map = Map.FromScenario(mapService.GetMapByScenarioName(game, currentState.Settings.TryGetValue("$map", out string scenarioName) ? scenarioName : string.Empty)),
            Settings = [],
            Game = game,
            Participants = [.. currentState.Participants.Select(MapProtoParticipant)],
            Team1 = MapProtoTeam(currentState.Teams[0]),
            Team2 = MapProtoTeam(currentState.Teams[1]),
        };

        var lobby = new MultiplayerLobby(browserLobby.Id, stream, client, setup, serverAPI, userService, companyService, mapService) {
            IsHost = false,
            IsReady = false, // Non-host participants are not considered ready until they explicitly mark themselves as ready
        };

        _ = lobby.SyncRemoteCompanies(); // Trigger initial sync/download of companies by other participants, but don't await it here as it may take some time and we want to return the lobby immediately

        return lobby;

    }

    private static Participant MapProtoParticipant(Proto.Lobbies.Participant participant) {
        return new Participant(0, participant.ParticipantId, participant.Name, participant.IsAi, participant.Ready);
    }

    private static Team MapProtoTeam(Proto.Lobbies.Team team) {
        var tType = team.Type.ToLowerInvariant() switch {
            "allies" => TeamType.Allies,
            "axis" => TeamType.Axis,
            _ => throw new InvalidOperationException($"Unknown team type: {team.Type}")
        };
        return new Team(tType, team.Alias, [.. team.Slots.Select(MapProtoSlot)]);
    }

    private static Team.Slot MapProtoSlot(Slot slot) {
        return new (slot.Id, slot.ParticipantId, slot.Faction, slot.CompanyId, AIDifficulty.FromName(slot.AiDifficulty), slot.Hidden, slot.Locked);
    }

}
