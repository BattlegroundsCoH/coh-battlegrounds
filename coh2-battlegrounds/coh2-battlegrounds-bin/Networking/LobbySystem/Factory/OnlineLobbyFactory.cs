using System;

using Battlegrounds.Errors.Networking;

using Battlegrounds.Logging;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.Server;
using Battlegrounds.Verification;
using Battlegrounds.Functional;
using Battlegrounds.Steam;
using Battlegrounds.Game;
using System.Reflection.Metadata;

namespace Battlegrounds.Networking.LobbySystem.Factory;

public sealed class OnlineLobbyFactory : ILobbyFactory {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly ServerAPI serverAPI;
    private readonly SteamUser user;

    public OnlineLobbyFactory(ServerAPI serverAPI, SteamUser user) {
        this.serverAPI = serverAPI;
        this.user = user;
    }

    public Result<ILobbyHandle?> HostLobby(string name, string? password, GameCase game, string mod) {
        return AsyncResult.Defer<ILobbyHandle>(provider => {

            // Create the intro message
            IntroMessage intro = new() {
                Type = 0,
                LobbyName = name,
                LobbyPassword = string.IsNullOrEmpty(password) ? string.Empty : password,
                PlayerUID = user.ID,
                PlayerName = user.Name,
                AppVersion = Integrity.IntegrityHashBytes,
                ModGuid = mod,
                Game = game.ToString(),
                InstanceType = 0,
                //IdentityKey =  // TODO: Implement
            };

            try {

                // Open connection
                ServerConnection? connection = ServerConnection.ConnectToServer(NetworkInterface.Endpoint.RemoteIPAddress, NetworkInterface.Endpoint.Tcp, intro, out ulong lobbyID)
                    ?? throw new ConnectionFailedException("Failed to establish TCP connection.");

                // Set API
                serverAPI.SetLobbyGuid(lobbyID);

                // Create handler
                OnlineLobbyHandle handle = new OnlineLobbyHandle(true, name, user, connection, serverAPI);

                // Return success
                provider.Success(handle);

            } catch (ConnectionFailedException connex) {

                // Log error
                logger.Exception(connex);

                // Log failure
                provider.Failure();

            } catch (Exception ex) {

                // Log error
                logger.Exception(ex);

                // Do failre
                provider.Failure();

            }

        });

    }

    public Result<ILobbyHandle?> JoinLobby(ServerLobby lobbyData, string password) {
        return AsyncResult.Defer<ILobbyHandle>(provider => {

            // Create intro
            IntroMessage intro = new() {
                Type = 1,
                LobbyUID = lobbyData.UID,
                LobbyPassword = password,
                PlayerName = user.Name,
                PlayerUID = user.ID,
                AppVersion = Integrity.IntegrityHashBytes
            };

            try {

                // Establish TCP connection
                ServerConnection? connection = ServerConnection.ConnectToServer(NetworkInterface.Endpoint.RemoteIPAddress, NetworkInterface.Endpoint.Tcp, intro, out ulong lobbyID)
                    ?? throw new ConnectionFailedException("Failed to establish TCP connection.");

                // Set API
                serverAPI.SetLobbyGuid(lobbyID);

                // Create handler
                OnlineLobbyHandle handle = new OnlineLobbyHandle(false, lobbyData.Name, user, connection, serverAPI);

                // Do success
                provider.Success(handle);

            } catch (ConnectionFailedException connex) {

                // Log error
                logger.Exception(connex);

                // Log failure
                provider.Failure();

            } catch (Exception ex) {

                // Log error
                logger.Exception(ex);

                // Do failre
                provider.Failure();

            }
        });
    }

}
