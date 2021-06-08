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

    public static class LobbyUtil {

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
                    success = true;
                } else {
                    success = false;
                }

            } catch (Exception ex) {

                // Log error
                Trace.WriteLine(ex, nameof(LobbyUtil));

            } finally {

                // Invoke creation
                onLobbyCreated?.Invoke(success, handler);

            }

        }

    }

}
