using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Threading;

namespace Battlegrounds.Online.Services {

    /// <summary>
    /// Instance of a lobby hub (Free service).
    /// </summary>
    public class LobbyHub {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanConnect() {

            Ping ping = new Ping();

            try {

                PingReply reply = ping.Send(BattlegroundsInstance.BattlegroundHubAddress);

                if (reply.Status == IPStatus.Success) {
                    return true;
                } else {
                    return false;
                }

            } catch { return false; }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ConnectableLobby> GetConnectableLobbies() {

            List<ConnectableLobby> connectableLobbies = new List<ConnectableLobby>();
            Message message = new Message(Message_Type.GET_LOBBIES);
            bool gotAll = false;
            int timeout = 20;

            void OnLobby(Socket x, Message y) {

                if (y.Argument2 == "True") {
                    MessageSender.WaitForMessage(x, OnLobby);
                }

                var match = Regex.Match(y.Argument1, @"\[(?<guid>\S{36});\""(?<name>(\S|\s)*)\"";(?<password>True|False)\]");

                connectableLobbies.Add(new ConnectableLobby {
                    lobby_guid = match.Groups["guid"].Value,
                    lobby_name = match.Groups["name"].Value,
                    lobby_passwordProtected = match.Groups["password"].Value.CompareTo("True") == 0,
                });

                if (y.Argument2 == "False") {
                    gotAll = true;
                }

            }

            MessageSender.SendMessage(new IPEndPoint(IPAddress.Parse(BattlegroundsInstance.BattlegroundHubAddress), 11000), message, OnLobby);
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
        /// 
        /// </summary>
        /// <param name="connectionResponse"></param>
        public void Connect(Action<bool, Connection> connectionResponse) {

            // The connect message
            Message addUserMessage = new Message(Message_Type.USER_SETUSERDATA, BattlegroundsInstance.LocalSteamuser.Name, BattlegroundsInstance.LocalSteamuser.ID.ToString());

            void OnResponse(Socket socket, Message response) {
                if (response.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                    Connection connection = new Connection(socket, false);
                    connectionResponse?.Invoke(true, connection);
                } else {
                    connectionResponse?.Invoke(false, null);
                }
            }

            // Send the message
            MessageSender.SendMessage(new IPEndPoint(IPAddress.Parse(BattlegroundsInstance.BattlegroundHubAddress), 11000), addUserMessage, OnResponse);

        }
        
    }

}
