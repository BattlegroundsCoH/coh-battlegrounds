using System;
using System.Diagnostics;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.LobbySystem.Roles.Host;
using Battlegrounds.Networking.LobbySystem.Roles.Participant;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;
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

            // Define handlers
            LobbyHandler handler = null;

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
                var broker = new BrokerHandler(connection, cachedPool, service);

                // Create network handler
                NetworkObjectHandler<ILobby> networkObject = new(lobby, broker, service, cachedPool);

                // Get self
                var self = lobby.CreateParticipant(steamUser.ID, steamUser.Name);

                // Create handler
                handler = new LobbyHandler(true, lobbyID) {
                    StaticInterface = service,
                    BrokerHandler = broker,
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

                // Create broker handler
                BrokerHandler broker = new(connection, cachedPool, service);

                // Create remote handle
                IRemoteHandle handle = new RemoteHandle(broker);

                // Get remote obj
                var result = broker.GetObjectIDByName("lobby");
                var lobID = result is IObjectID id ? id : throw new ConnectionFailedException("Failed to establish connection with remote lobby instance.");

                // Create lobby
                RemoteLobby lobby = new(lobID, handle);
                lobby.InitRemote();

                // Create network handler
                NetworkObjectHandler<ILobby> networkObject = new(lobby, broker, service, cachedPool);
                SetupParticipantNetworkObjectHandler(networkObject);

                // Set handler
                handler = new LobbyHandler(false, lobbyID) {
                    StaticInterface = null,
                    BrokerHandler = broker,
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

        private static void SetupParticipantNetworkObjectHandler(NetworkObjectHandler<ILobby> networkObjectHandler) {

            // Add methods to filter when observing.
            networkObjectHandler.Observer.MethodFilter.Add(nameof(ILobby.RemoveParticipant));
            networkObjectHandler.Observer.MethodFilter.Add(nameof(ILobby.CreateAIParticipant));
            networkObjectHandler.Observer.MethodFilter.Add(nameof(ILobby.CreateParticipant));

        }

    }

}
