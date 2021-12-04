using System;
using System.Diagnostics;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem {

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
        public static void HostLobby(ServerAPI serverAPI, string lobbyName, string lobbyPassword, Action<bool, LobbyAPI> onLobbyCreated) {

            // Get steam user
            var steamUser = BattlegroundsInstance.Steam.User;

            // Log in with auth service
            AuthService.Login(steamUser.ID, steamUser.Name);

            // Success flag
            bool success = false;

            // Define handlers
            LobbyAPI handle = null;

            try {

                // Create intro
                IntroMessage intro = new() { 
                    Host = true, 
                    LobbyName = lobbyName, LobbyPassword = lobbyPassword, 
                    PlayerUID = steamUser.ID, PlayerName = steamUser.Name
                };

                // Establish connection
                ServerConnection connection = ServerConnection.ConnectToServer(NetworkInterface.GetBestAddress(), 11000, intro, out ulong lobbyID);
                if (connection is null) {
                    throw new ConnectionFailedException("Failed to establish TCP connection.");
                }

                // Set API
                serverAPI.SetLobbyGuid(lobbyID);
                
                // Create handler
                handle = new LobbyAPI(true, steamUser, connection, serverAPI);

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

        public static void JoinLobby(ServerAPI serverAPI, ServerLobby lobbyData, string password, Action<bool, LobbyAPI> onLobbyJoined) {

            const string methoddb = $"{nameof(LobbyUtil)}::{nameof(JoinLobby)}";

            // Get steam user
            var steamUser = BattlegroundsInstance.Steam.User;

            // Log in with auth service
            AuthService.Login(steamUser.ID, steamUser.Name);

            // Success flag
            bool success = false;

            // Define handler
            LobbyAPI handle = null;

            try {

                // Create intro
                IntroMessage intro = new() {
                    Host = false,
                    LobbyUID = lobbyData.UID,
                    LobbyPassword = password,
                    PlayerName = steamUser.Name,
                    PlayerUID = steamUser.ID
                };

                // Establish TCP connection
                ServerConnection connection = ServerConnection.ConnectToServer(NetworkInterface.GetBestAddress(), 11000, intro, out ulong lobbyID);
                if (connection is null) {
                    throw new ConnectionFailedException("Failed to establish TCP connection.");
                }

                // Set API
                serverAPI.SetLobbyGuid(lobbyID);

                // Create handler
                handle = new LobbyAPI(false, steamUser, connection, serverAPI);

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

}
