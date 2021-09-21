using System;
using System.Diagnostics;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.LobbySystem.Roles.Host;
using Battlegrounds.Networking.LobbySystem.Roles.Participant;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

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
        public static void HostLobby(ServerAPI serverAPI, string lobbyName, string lobbyPassword, Action<bool, LobbyHandler> onLobbyCreated) {

            // Get steam user
            var steamUser = BattlegroundsInstance.Steam.User;

            // Log in with auth service
            AuthService.Login(steamUser.ID, steamUser.Name);

            // Machine-specific data
            ObjectCachedPool cachedPool = new();
            LobbyService service = new(cachedPool);
            var lobID = service.Create<HostedLobby>("lobby", lobbyName);

            // Create lobby
            var lobby = (cachedPool.Get(lobID) as CachedObject).Obj<HostedLobby>();

            // Success flag
            bool success = false;

            // Define handler
            LobbyHandler handler = null;

            try {

                // Create intro
                IntroMessage intro = new(true, lobbyName, lobbyPassword, 1);

                // Establish connection
                SocketConnection connection = SocketConnection.EstablishConnection(NetworkInterface.GetBestAddress(), 11000, steamUser.ID, intro);
                if (connection is null) {
                    throw new ConnectionFailedException("Failed to establish TCP connection.");
                }

                // Set API
                serverAPI.SetLobbyGuid(connection.ConnectionID);

                // Create network handler
                NetworkObjectHandler<ILobby> networkObject = new(lobby, connection, service, cachedPool);

                // Get self
                var self = lobby.CreateParticipant(steamUser.ID, steamUser.Name);

                // Create handler
                handler = new LobbyHandler(true) {
                    StaticInterface = service,
                    Connection = connection,
                    ObjectPool = cachedPool,
                    LobbyID = lobID,
                    Lobby = lobby,
                    Self = self,
                };

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
                onLobbyCreated?.Invoke(success, handler);

            }

        }

        public static void JoinLobby(ServerAPI serverAPI, ServerLobby lobbyData, string password, Action<bool, LobbyHandler> onLobbyJoined) {

            const string methoddb = $"{nameof(LobbyUtil)}::{nameof(JoinLobby)}";

            // Get steam user
            var steamUser = BattlegroundsInstance.Steam.User;

            // Log in with auth service
            AuthService.Login(steamUser.ID, steamUser.Name);

            // Machine-specific data
            ObjectCachedPool cachedPool = new();
            LobbyService service = new(cachedPool);

            // Success flag
            bool success = false;

            // Define handler
            LobbyHandler handler = null;

            try {

                // Create intro
                IntroMessage intro = new(false, lobbyData.Guid, password, 1);

                // Establish TCP connection
                SocketConnection connection = SocketConnection.EstablishConnection(NetworkInterface.GetBestAddress(), 11000, steamUser.ID, intro);
                if (connection is null) {
                    throw new ConnectionFailedException("Failed to establish TCP connection.");
                }

                // Set API
                serverAPI.SetLobbyGuid(connection.ConnectionID);

                // Get remote obj
                var result = connection.SendMessage(new GetObjectMessage("lobby", true), true);
                var lobID = result is IDMessage id ? id.ID : throw new ConnectionFailedException("Failed to establish connection with remote lobby instance.");

                // Create lobby
                RemoteLobby lobby = new(lobID);

                // Create network handler
                NetworkObjectHandler<ILobby> networkObject = new(lobby, connection, service, cachedPool);

                // Set handler
                handler = new LobbyHandler(false) {
                    StaticInterface = null,
                    Connection = connection,
                    ObjectPool = cachedPool,
                    Lobby = lobby,
                    LobbyID = lobID
                };

                // Set success flag
                success = true;

                // Set sucess flag
                Trace.WriteLine($"Sucessfully joined lobby [lobby = {handler.Lobby is not null}, self = {handler.Self is not null}, self machine = {handler.Self?.IsSelf}]", methoddb);

            } catch (ConnectionFailedException connex) {

                // Log error
                Trace.WriteLine(connex, methoddb);

                // Propogate -> Let GUI handle this
                throw;

            } catch (Exception ex) {

                // Log error
                Trace.WriteLine(ex, methoddb);

            } finally {
                onLobbyJoined?.Invoke(success, handler);
            }

        }

    }

}
