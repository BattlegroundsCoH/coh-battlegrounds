using System;
using System.Collections.Generic;
using System.Text;

using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Services;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Models.Lobbies;


public sealed class MultiplayerLobbyFactory(IServiceProvider serviceProvider) {

    public async Task<MultiplayerLobby> GetLobby(Proto.Lobbies.LobbyService.LobbyServiceClient client, AsyncServerStreamingCall<LobbyStateUpdate> stream, LobbySetup setup) {
        var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        var serverAPI = provider.GetRequiredService<IBattlegroundsServerAPI>();
        var companyService = provider.GetRequiredService<ICompanyService>();
        var userService = provider.GetRequiredService<IUserService>();

        if (!await stream.ResponseStream.MoveNext()) {
            throw new InvalidOperationException("Failed to start lobby. No response received from server.");
        }

        // Await for the first response to get the lobby ID
        var hostResponse = stream.ResponseStream.Current;

        var lobby = new MultiplayerLobby(hostResponse.LobbyId, stream, client, setup, serverAPI, userService, companyService) {
            IsHost = true, // The host is the one who created the lobby
        };

        return lobby;

    }

}
