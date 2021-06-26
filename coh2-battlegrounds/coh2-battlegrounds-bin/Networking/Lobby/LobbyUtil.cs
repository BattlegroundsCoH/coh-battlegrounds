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

                // Establish connection
                TcpConnection connection = TcpConnection.EstablishConnectionTo(NetworkingInstance.GetBestAddress(), 11000, steamUser.ID);

                // Create handler
                HostRequestHandler requestHandler = new HostRequestHandler(connection, service, cachedPool);
                lobby.RequestHandler = requestHandler;

                // Set connection
                connection.SetRequestHandler(requestHandler);
                IMessage response = connection.SendMessage(new IntroMessage(true, lobbyName, lobbyPassword, 1), true);

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

                // Check response
                if (response is StringMessage msg) {
                    if (msg.Message != "OK") {
                        Trace.WriteLine(msg?.Message, nameof(LobbyUtil));
                        success = false;
                    }
                    lobby.SetState(LobbyState.LOBBY_INLOBBY);
                    success = true;
                } else {
                    success = false;
                }

            } catch (Exception ex) {

                // Log error
                Trace.WriteLine(ex, $"{nameof(LobbyUtil)}::{nameof(HostLobby)}");

            } finally {

                // Invoke creation
                onLobbyCreated?.Invoke(success, handler);

            }

        }

        public static void JoinLobby(ServerAPI serverAPI, ServerLobby lobby, string password, Action<bool, LobbyHandler> onLobbyJoined) {

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

                // Establish TCP connection
                TcpConnection connection = TcpConnection.EstablishConnectionTo(NetworkingInstance.GetBestAddress(), 11000, steamUser.ID);

                // Create handler
                ParticipantRequestHandler participantHandler = new ParticipantRequestHandler(connection, instanceHandler) {
                    DependencyInjector = (t, objectID, requestHandler) => t switch {
                        nameof(HostedLobby) => new ProxyLobby(objectID, requestHandler, serverAPI),
                        nameof(LobbyMember) => new ProxyLobbyMember(objectID, requestHandler, new AuthObject(steamUser.ID, steamUser.Name)),
                        nameof(LobbyTeam) => new ProxyLobbyTeam(objectID, requestHandler),
                        _ => null
                    }
                };

                // Set request handler
                connection.SetRequestHandler(participantHandler);
                
                // Send intro message
                if (connection.SendMessage(new IntroMessage(false, lobby.Name, password, 1), true) is StringMessage str) {
                    if (str.Message != "OK") {
                        throw new Exception(str.Message);
                    }
                } else {
                    throw new Exception("Failed to get response from server");
                }

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
                };

                // Set success flag
                success = true;

            } catch (Exception ex) {

                // Log error
                Trace.WriteLine(ex, $"{nameof(LobbyUtil)}::{nameof(JoinLobby)}");

            } finally {
                onLobbyJoined?.Invoke(success, handler);
            }

        }

    }

}
