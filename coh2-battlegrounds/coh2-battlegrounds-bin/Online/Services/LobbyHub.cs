using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Threading;

using Battlegrounds.Steam;

namespace Battlegrounds.Online.Services {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isConnected"></param>
    /// <param name="connection"></param>
    public delegate void LobbyHubConnectionHandler(bool isConnected, Connection connection);

    /// <summary>
    /// Instance of a lobby hub (Free service).
    /// </summary>
    public class LobbyHub {

        /// <summary>
        /// The <see cref="SteamUser"/> who will be connecting to the <see cref="LobbyHub"/>.
        /// </summary>
        /// <remarks>
        /// At no point are users verified on the server - Personal Security risk!!!
        /// </remarks>
        public SteamUser User { get; set; }

        /// <summary>
        /// Can the local machine establish a connection to the server machine.
        /// </summary>
        /// <remarks>This method does not guarantee server connection. Only that the server machine is active.</remarks>
        /// <returns>True if the server machine was pinged with success. False if no ping-back was received.</returns>
        public static bool CanConnect() {

            try {

                Ping ping = new Ping();
                PingReply reply = ping.Send(AddressBook.LobbyServerAddress);

                if (reply.Status == IPStatus.Success) {
                    return true;
                } else {
                    return false;
                }

            } catch { return false; }

        }

        private static ConnectableLobby GetLobby(string serverStringResponse) {

            var match = Regex.Match(serverStringResponse, @"\[(?<guid>\S{36});\""(?<name>(\S|\s)*)\"";(?<password>True|False)\]");

            if (match.Success) {
                return new ConnectableLobby(match.Groups["guid"].Value, match.Groups["name"].Value, match.Groups["password"].Value.CompareTo("True") == 0);
            } else {
                return null;
            }

        }

        /// <summary>
        /// Get a list of all <see cref="ConnectableLobby"/> instances available on the server in a synchronous manner.
        /// </summary>
        /// <remarks>Blocks execution until all <see cref="ConnectableLobby"/> instances have been received.</remarks>
        /// <returns>A list of <see cref="ConnectableLobby"/> instances representing the lobbies on the server.</returns>
        public List<ConnectableLobby> GetConnectableLobbies(bool fetchLobbyData = true) {

            List<ConnectableLobby> connectableLobbies = new List<ConnectableLobby>();
            Message message = new Message(MessageType.GET_LOBBIES);
            bool gotAll = false;
            int timeout = 20;

            void OnLobby(Socket x, Message y) {

                if (y.Argument2 == "True") {
                    MessageSender.WaitForMessage(x, OnLobby);
                }

                if (GetLobby(y.Argument1) is ConnectableLobby lobby) {
                    if (fetchLobbyData) {
                        lobby.Update(x => connectableLobbies?.Add(x));
                    } else {
                        connectableLobbies.Add(lobby);
                    }
                }

                if (y.Argument2 == "False") {
                    gotAll = true;
                }

            }

            MessageSender.SendMessage(AddressBook.GetLobbyServer(), message, OnLobby);
            int m = connectableLobbies.Count;

            while (!gotAll && timeout > 0) {
                Console.WriteLine("Wating for lobby list...");
                Thread.Sleep(100);
                if (m == connectableLobbies.Count) {
                    timeout--;
                } else {
                    m = connectableLobbies.Count;
                }
            }

            return connectableLobbies;

        }

        /// <summary>
        /// Get a list of all <see cref="ConnectableLobby"/> instances available on the server in an asynchronous manner.
        /// </summary>
        /// <param name="callback">The callback to invoke wheneer a new <see cref="ConnectableLobby"/> instance was discovered.</param>
        public void GetConnectableLobbies(Action<ConnectableLobby> callback, bool fetchLobbyData = true) {

            Message message = new Message(MessageType.GET_LOBBIES);

            void OnLobby(Socket x, Message y) {

                if (y.Argument2 == "True") {
                    MessageSender.WaitForMessage(x, OnLobby);
                }

                if (GetLobby(y.Argument1) is ConnectableLobby lobby) {
                    if (fetchLobbyData) {
                        lobby.Update(callback);
                    } else {
                        callback?.Invoke(lobby);
                    }
                }

            }

            MessageSender.SendMessage(AddressBook.GetLobbyServer(), message, OnLobby);

        }

        /// <summary>
        /// Establish a continuous connection to the server.
        /// </summary>
        /// <param name="connectionResponse">The server response to connection attempt.</param>
        /// <exception cref="ArgumentNullException"/>
        public void Connect(LobbyHubConnectionHandler connectionResponse) {

            // Make sure there's a user to work with
            if (this.User is null) {
                throw new ArgumentNullException(nameof(this.User), "Steam User object not assigned!");
            }

            // The connect message
            Message addUserMessage = new Message(MessageType.USER_SETUSERDATA, this.User.Name, this.User.ID.ToString());

            void OnResponse(Socket socket, Message response) {
                if (response.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                    Connection connection = new Connection(socket, false);
                    connectionResponse?.Invoke(true, connection);
                } else {
                    connectionResponse?.Invoke(false, null);
                }
            }

            // Send the message
            MessageSender.SendMessage(AddressBook.GetLobbyServer(), addUserMessage, OnResponse);

        }

        /// <summary>
        /// Reconnect to a lobby if connection to server was lost.
        /// </summary>
        /// <param name="existingConnection">The existing <see cref="Connection"/> object.</param>
        /// <param name="user">The <see cref="SteamUser"/> to reconnect to the lobby.</param>
        /// <param name="lobbyID">The GUID of the lobby to try and reconnect with.</param>
        /// <param name="lobbypswd">The password to use when connecting to the lobby.</param>
        /// <param name="isHost">Inform the lobby we wish to reconnect as host.</param>
        /// <param name="connectionResponse">The <see cref="LobbyHubConnectionHandler"/> to handle the final server response.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public static void Reconnect(Connection existingConnection, SteamUser user, string lobbyID, string lobbypswd, bool isHost, LobbyHubConnectionHandler connectionResponse) {

            // Make sure we're not connected
            if (existingConnection.IsConnected) {
                throw new InvalidOperationException("Cannot reconnect to server lobby if already connected!");
            }

            // Make sure there's a user to work with
            if (user is null) {
                throw new ArgumentNullException(nameof(user), "Steam User object not assigned!");
            }

            // The connect message
            Message addUserMessage = new Message(MessageType.USER_SETUSERDATA, user.Name, user.ID.ToString());

            // On Reconnected handler
            void OnReconnect(Message message) {
                if (message.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                    connectionResponse?.Invoke(true, existingConnection);
                } else {
                    connectionResponse?.Invoke(false, existingConnection);
                }
            }

            // On server response handler
            void OnResponse(Socket socket, Message response) {
                if (response.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                    existingConnection.UpdateSocket(socket);
                    existingConnection.SendMessageWithResponse(new Message(MessageType.LOBBY_TRYRECONNECT, lobbyID, lobbypswd, isHost.ToString()), OnReconnect);
                } else {
                    connectionResponse?.Invoke(false, null);
                }
            }

            // Send the message
            MessageSender.SendMessage(AddressBook.GetLobbyServer(), addUserMessage, OnResponse);

        }

    }

}
