using System;
using System.Diagnostics;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public static class LobbyUtil {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverAPI"></param>
    /// <param name="lobbyName"></param>
    /// <param name="lobbyPassword"></param>
    /// <param name="onLobbyCreated"></param>
    public static void HostLobby(ServerAPI serverAPI, string lobbyName, string lobbyPassword, LobbyConnectCallback onLobbyCreated) {

        // Get steam user
        var steamUser = BattlegroundsContext.Steam.User;

        // Success flag
        bool success = false;

        // Define handlers
        OnlineLobbyHandle? handle = null;

        try {

            // Create intro
            IntroMessage intro = new() { 
                Type = 0, 
                LobbyName = lobbyName, LobbyPassword = lobbyPassword, 
                PlayerUID = steamUser.ID, PlayerName = steamUser.Name
            };

            // Establish connection
            ServerConnection? connection = ServerConnection.ConnectToServer(NetworkInterface.Endpoint.RemoteIPAddress, NetworkInterface.Endpoint.Tcp, intro, out ulong lobbyID);
            if (connection is null) {
                throw new ConnectionFailedException("Failed to establish TCP connection.");
            }

            // Set API
            serverAPI.SetLobbyGuid(lobbyID);
            
            // Create handler
            handle = new OnlineLobbyHandle(true, lobbyName, steamUser, connection, serverAPI);

            // Set success flag
            success = true;

        } catch (ConnectionFailedException connex) {

            // Log error
            Trace.WriteLine(connex, $"{nameof(LobbyUtil)}::{nameof(HostLobby)}");

            // Propogate -> Let GUI handle this
            throw;

        } catch (Exception ex) {

            // Log error
            Trace.WriteLine(ex, $"{nameof(LobbyUtil)}::{nameof(HostLobby)}");

        } finally {

            // Invoke creation
            onLobbyCreated?.Invoke(success, handle);

        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverAPI"></param>
    /// <param name="lobbyData"></param>
    /// <param name="password"></param>
    /// <param name="onLobbyJoined"></param>
    public static void JoinLobby(ServerAPI serverAPI, ServerLobby lobbyData, string password, LobbyConnectCallback onLobbyJoined) {

        const string methoddb = $"{nameof(LobbyUtil)}::{nameof(JoinLobby)}";

        // Get steam user
        var steamUser = BattlegroundsContext.Steam.User;

        // Success flag
        bool success = false;

        // Define handler
        OnlineLobbyHandle? handle = null;

        try {

            // Create intro
            IntroMessage intro = new() {
                Type = 1,
                LobbyUID = lobbyData.UID,
                LobbyPassword = password,
                PlayerName = steamUser.Name,
                PlayerUID = steamUser.ID
            };

            // Establish TCP connection
            ServerConnection? connection = ServerConnection.ConnectToServer(NetworkInterface.Endpoint.RemoteIPAddress, NetworkInterface.Endpoint.Tcp, intro, out ulong lobbyID);
            if (connection is null) {
                throw new ConnectionFailedException("Failed to establish TCP connection.");
            }

            // Set API
            serverAPI.SetLobbyGuid(lobbyID);

            // Create handler
            handle = new OnlineLobbyHandle(false, lobbyData.Name, steamUser, connection, serverAPI);

            // Set success flag
            success = true;
            
        } catch (ConnectionFailedException connex) {

            // Log error
            Trace.WriteLine(connex, methoddb);

            // Propogate -> Let GUI handle this
            throw;

        } catch (Exception ex) {

            // Log error
            Trace.WriteLine(ex, methoddb);

        } finally {
            onLobbyJoined?.Invoke(success, handle);
        }

    }

}
