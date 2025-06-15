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

namespace Battlegrounds.Networking.LobbySystem.Factory;

/// <summary>
/// Represents a factory for creating and managing online lobbies.
/// </summary>
public sealed class OnlineLobbyFactory : ILobbyFactory {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly NetworkEndpoint networkEndpoint;
    private readonly ServerAPI serverAPI;
    private readonly SteamUser user;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnlineLobbyFactory"/> class with the specified dependencies.
    /// </summary>
    /// <param name="serverAPI">The API for communication with the server.</param>
    /// <param name="endpoint">The network endpoint used to establish the connection with the server.</param>
    /// <param name="user">The Steam user associated with this lobby factory.</param>
    public OnlineLobbyFactory(ServerAPI serverAPI, NetworkEndpoint endpoint, SteamUser user) {
        this.networkEndpoint = endpoint;
        this.serverAPI = serverAPI;
        this.user = user;
    }

    /// <summary>
    /// Hosts a new online lobby with the given parameters.
    /// </summary>
    /// <param name="name">The name of the lobby.</param>
    /// <param name="password">Optional password for the lobby.</param>
    /// <param name="game">The game case for the lobby.</param>
    /// <param name="mod">The mod associated with the game.</param>
    /// <returns>
    /// A result of the asynchronous operation that, when successful, contains an <see cref="ILobbyHandle"/>
    /// representing the hosted lobby; otherwise, it returns null indicating a failure to create the lobby.
    /// </returns>
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
                ServerConnection? connection = ServerConnection.ConnectToServer(networkEndpoint.RemoteIPAddress, networkEndpoint.Tcp, intro, out ulong lobbyID)
                    ?? throw new ConnectionFailedException("Failed to establish TCP connection.");

                // Set API
                serverAPI.SetLobbyGuid(lobbyID);

                // Create handler
                OnlineLobbyHandle handle = new OnlineLobbyHandle(true, name, game, user, connection, serverAPI);

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

    /// <summary>
    /// Joins an existing online lobby with the given parameters.
    /// </summary>
    /// <param name="lobbyData">The data of the lobby to join.</param>
    /// <param name="password">The password required to join the lobby.</param>
    /// <returns>
    /// A result of the asynchronous operation that, when successful, contains an <see cref="ILobbyHandle"/>
    /// representing the joined lobby; otherwise, it returns null indicating a failure to join the lobby.
    /// </returns>
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
                ServerConnection? connection = ServerConnection.ConnectToServer(networkEndpoint.RemoteIPAddress, networkEndpoint.Tcp, intro, out ulong lobbyID)
                    ?? throw new ConnectionFailedException("Failed to establish TCP connection.");

                // Set API
                serverAPI.SetLobbyGuid(lobbyID);

                // Create handler
                OnlineLobbyHandle handle = new OnlineLobbyHandle(false, lobbyData.Name, lobbyData.GetGame(), user, connection, serverAPI);

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
