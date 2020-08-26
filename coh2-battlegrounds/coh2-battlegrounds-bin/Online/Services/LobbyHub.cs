using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Threading;

using Battlegrounds.Steam;

namespace Battlegrounds.Online.Services {

    /// <summary>
    /// Instance of a lobby hub (Free service).
    /// </summary>
    public class LobbyHub {

        /// <summary>
        /// The <see cref="SteamUser"/> who will be connecting to the <see cref="LobbyHub"/>.<br/>At no point are users verified on the server - Security risk!!!
        /// </summary>
        public SteamUser User { get; set; }

        /// <summary>
        /// Can the local machine establish a connection to the server machine.
        /// </summary>
        /// <remarks>This method does not guarantee server connection. Only that the server machine is active.</remarks>
        /// <returns>True if the server machine was pinged with success. False if no ping-back was received.</returns>
        public bool CanConnect() {

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

        private ConnectableLobby GetLobby(string serverStringResponse) {

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
            Message message = new Message(Message_Type.GET_LOBBIES);
            bool gotAll = false;
            int timeout = 20;

            void OnLobby(Socket x, Message y) {

                if (y.Argument2 == "True") {
                    MessageSender.WaitForMessage(x, OnLobby);
                }

                if (this.GetLobby(y.Argument1) is ConnectableLobby lobby) {
                    connectableLobbies.Add(lobby);
                    if (fetchLobbyData) {
                        lobby.Update();
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

            Message message = new Message(Message_Type.GET_LOBBIES);

            void OnLobby(Socket x, Message y) {

                if (y.Argument2 == "True") {
                    MessageSender.WaitForMessage(x, OnLobby);
                }

                if (this.GetLobby(y.Argument1) is ConnectableLobby lobby) {
                    callback?.Invoke(lobby);
                    if (fetchLobbyData) {
                        lobby.Update();
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
        public void Connect(Action<bool, Connection> connectionResponse) {

            // Make sure there's a user to work with
            if (this.User is null) {
                throw new ArgumentNullException("Steam User object not assigned!");
            }

            // The connect message
            Message addUserMessage = new Message(Message_Type.USER_SETUSERDATA, this.User.Name, this.User.ID.ToString());

            void OnResponse(Socket socket, Message response) {
                if (response.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                    Connection connection = new Connection(socket, false);
                    connectionResponse?.Invoke(true, connection);
                } else {
                    connectionResponse?.Invoke(false, null);
                }
            }

            // Send the message
            MessageSender.SendMessage(AddressBook.GetLobbyServer(), addUserMessage, OnResponse);

        }
        
    }

}
