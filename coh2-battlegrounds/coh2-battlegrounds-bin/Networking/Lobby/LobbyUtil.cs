using System;
using System.Diagnostics;

using Battlegrounds.Networking.Communication;
using Battlegrounds.Networking.Communication.Messaging;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Reflection;
using Battlegrounds.Networking.Requests;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

namespace Battlegrounds.Networking.Lobby {

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
            SteamUser steamUser = BattlegroundsInstance.Steam.User;

            // Log in with auth service
            AuthService.Login(steamUser.ID, steamUser.Name);

            // Machine-specific data
            ObjectCachedPool cachedPool = new ObjectCachedPool();
            LobbyService service = new LobbyService(cachedPool);
            IObjectID lobID = service.Create<HostedLobby>("lobby", lobbyName, serverAPI);

            // Create lobby
            HostedLobby lobby = (cachedPool.Get(lobID) as CachedObject).Obj<HostedLobby>();
            lobby.AllowSpectators = true;
            lobby.AutoAssignTeams = true;

            // Get self
            ILobbyMember self = lobby.Join(steamUser.ID, steamUser.Name);

            // Success flag
            bool success = false;

            // Define handler
            LobbyHandler handler = null;

            try {

                // Create intro
                var intro = new IntroMessage(true, lobbyName, lobbyPassword, 1);

                // Establish connection
                HttpConnection connection = HttpConnection.EstablishConnection(NetworkingInstance.GetBestAddress(), 11000, steamUser.ID, intro);
                if (connection is null) {
                    throw new Exception("Failed to establish HTTP connection.");
                }

                // Create handler
                HostRequestHandler requestHandler = new HostRequestHandler(connection, service, cachedPool);
                lobby.RequestHandler = requestHandler;

                // Set connection
                connection.SetRequestHandler(requestHandler);

                // Create handler
                handler = new LobbyHandler(serverAPI, true) {
                    RequestHandler = requestHandler,
                    StaticInterface = service,
                    Connection = connection,
                    ObjectPool = cachedPool,
                    LobbyID = lobID,
                    Lobby = lobby,
                    Self = self,
                };

                lobby.SetState(LobbyState.LOBBY_INLOBBY);
                success = true;

            } catch (Exception ex) {

                // Log error
                Trace.WriteLine(ex, $"{nameof(LobbyUtil)}::{nameof(HostLobby)}");

            } finally {

                // Invoke creation
                onLobbyCreated?.Invoke(success, handler);

            }

        }

        public static void JoinLobby(ServerAPI serverAPI, ServerLobby lobby, string password, Action<bool, LobbyHandler> onLobbyJoined) {

            const string methoddb = $"{nameof(LobbyUtil)}::{nameof(JoinLobby)}";

            // Get steam user
            SteamUser steamUser = BattlegroundsInstance.Steam.User;

            // Log in with auth service
            AuthService.Login(steamUser.ID, steamUser.Name);

            // Success flag
            bool success = false;

            // Define handler
            LobbyHandler handler = null;

            try {

                // Create instance handler
                ObjectInstanceHandler instanceHandler = new ObjectInstanceHandler();

                // Create intro
                var intro = new IntroMessage(false, lobby.Guid, password, 1);

                // Establish TCP connection
                HttpConnection connection = HttpConnection.EstablishConnection(NetworkingInstance.GetBestAddress(), 11000, steamUser.ID, intro);
                if (connection is null) {
                    throw new Exception("Failed to establish HTTP connection.");
                }

                // Create handler
                ParticipantRequestHandler participantHandler = new ParticipantRequestHandler(connection, instanceHandler) {
                    DependencyInjector = (t, objectID, requestHandler) => t switch {
                        nameof(HostedLobby) => new ProxyLobby(objectID, requestHandler, serverAPI),
                        nameof(LobbyMember) => new ProxyLobbyMember(objectID, requestHandler, new AuthObject(steamUser.ID, steamUser.Name)),
                        nameof(LobbyTeam) => new ProxyLobbyTeam(objectID, requestHandler),
                        nameof(LobbyTeamSlot) => new ProxyLobbyTeamSlot(requestHandler, objectID),
                        _ => null
                    }
                };

                // Set request handler
                connection.SetRequestHandler(participantHandler);
                
                // Send intro message
                /*if (connection.SendMessage(new IntroMessage(false, lobby.Name, password, 1), true) is StringMessage str) {
                    if (str.Message != "OK") {
                        throw new Exception(str.Message);
                    }
                } else {
                    throw new Exception("Failed to get response from server");
                }*/

                // Get proxy
                var proxyObj = participantHandler.SendRequest(null, "lobby");
                instanceHandler.RegisterInstance(proxyObj as ProxyLobby);

                // Get self
                ILobbyMember self = (proxyObj as ILobby).Join(steamUser.ID, steamUser.Name);

                // Set handler
                handler = new LobbyHandler(serverAPI, false) {
                    RequestHandler = participantHandler,
                    StaticInterface = null,
                    Connection = connection,
                    ObjectPool = null,
                    Lobby = proxyObj as ILobby,
                    Self = self,
                    InstanceHandler = instanceHandler,
                    LobbyID = (proxyObj as ProxyLobby).ObjectID
                };

                // Set success flag
                success = true;

                Trace.WriteLine($"Sucessfully joined lobby [lobby = {handler.Lobby is not null}, self = {handler.Self is not null}, self machine = {handler.Self.IsLocalMachine}]", methoddb);

            } catch (Exception ex) {

                // Log error
                Trace.WriteLine(ex, methoddb);

            } finally {
                onLobbyJoined?.Invoke(success, handler);
            }

        }

    }

}
