using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class ManagedLobby {

        /// <summary>
        /// 
        /// </summary>
        public const string SEND_ALL = "ALL";

        Connection m_underlyingConnection;
        bool m_isHost;

        /// <summary>
        /// 
        /// </summary>
        public event ManagedLobbyPlayerEvent OnPlayerEvent;

        /// <summary>
        /// 
        /// </summary>
        public event ManagedLobbyLocalEvent OnLocalEvent;

        /// <summary>
        /// 
        /// </summary>
        public event ManagedLobbyQuery OnFileRequest;

        /// <summary>
        /// 
        /// </summary>
        public event ManagedLobbyQuery OnDataRequest;

        /// <summary>
        /// 
        /// </summary>
        public event ManagedLobbyFileReceived OnFileReceived;

        /// <summary>
        /// 
        /// </summary>
        public bool IsHost => m_isHost;

        private ManagedLobby(Connection connection, bool isHost) {

            // Assign the underlying connection and start listening for messages
            this.m_underlyingConnection = connection;
            this.m_underlyingConnection.OnMessage += this.ManagedLobbyInternal_MessageReceived;
            this.m_underlyingConnection.Start();

            // Assign hostship
            this.m_isHost = isHost;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatMessage"></param>
        public void SendChatMessage(string chatMessage) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_CHATMESSAGE, chatMessage));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaMessage"></param>
        public void SendMetaMessage(string metaMessage) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_METAMESSAGE, metaMessage));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyInformation"></param>
        /// <param name="reponse"></param>
        public void GetLobbyInformation(string lobbyInformation, ManagedLobbyQueryResponse reponse) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyInformation"></param>
        /// <param name="lobbyInformationValue"></param>
        public void SetLobbyInformation(string lobbyInformation, string lobbyInformationValue) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="filepath"></param>
        public void SendFile(string receiver, string filepath)
            => this.SendFile(receiver, filepath, -1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="filepath"></param>
        /// <param name="identifier"></param>
        public void SendFile(string receiver, string filepath, int identifier) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                m_underlyingConnection.SendFile(receiver, filepath, identifier);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="response"></param>
        public void GetCompanyFileFrom(string from, ManagedLobbyFileReceived response) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                Message message = new Message(Message_Type.LOBBY_REQUEST_COMPANY, from);
                Message.SetIdentifier(m_underlyingConnection.ConnectionSocket, message);
                void OnMessage(Message msg) {
                    if (msg.Descriptor == Message_Type.LOBBY_SENDFILE) {
                        response?.Invoke(from, true, msg.FileData);
                    } else {
                        response?.Invoke(from, false, null);
                    }
                }
                m_underlyingConnection.SetIdentifierReceiver(message.Identifier, OnMessage);
                m_underlyingConnection.SendMessage(message);
                Console.WriteLine("Sent 'GetCompanyFileFrom' request with ID " + message.Identifier);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Leave() {
            if (m_underlyingConnection != null) {
                m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_LEAVE));
                m_underlyingConnection.Stop();
                m_underlyingConnection = null;
            }
        }

        private void ManagedLobbyInternal_MessageReceived(Message incomingMessage) {
            switch (incomingMessage.Descriptor) {
                case Message_Type.LOBBY_CHATMESSAGE:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Message, incomingMessage.Argument2, incomingMessage.Argument1);
                    break;
                case Message_Type.LOBBY_METAMESSAGE:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Meta, incomingMessage.Argument2, incomingMessage.Argument1);
                    break;
                case Message_Type.LOBBY_JOIN:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Join, incomingMessage.Argument1, string.Empty);
                    break;
                case Message_Type.LOBBY_LEAVE:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Leave, incomingMessage.Argument1, string.Empty);
                    break;
                case Message_Type.LOBBY_KICK:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Kicked, incomingMessage.Argument1, incomingMessage.Argument2);
                    break;
                case Message_Type.LOBBY_KICKED:
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.KICKED, incomingMessage.Argument1);
                    break;
                case Message_Type.LOBBY_SETHOST:
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.HOST, string.Empty);
                    break;
                case Message_Type.LOBBY_REQUEST_COMPANY:
                    this.OnFileRequest?.Invoke(true, incomingMessage.Argument1, "CompanyData", incomingMessage.Identifier);
                    break;
                case Message_Type.LOBBY_REQUEST_RESULTS:
                    this.OnFileRequest?.Invoke(true, incomingMessage.Argument1, "MatchData", incomingMessage.Identifier);
                    break;
                default: Console.WriteLine(incomingMessage.Descriptor + ":" + incomingMessage.Identifier); break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="lobbyName"></param>
        /// <param name="lobbyPassword"></param>
        /// <param name="managedCallback"></param>
        public static void Host(LobbyHub hub, string lobbyName, string lobbyPassword, ManagedLobbyConnectCallback managedCallback) {

            // Callback for establishing connection
            void OnConnectionEstablished(bool connected, Connection connection) {
                if (connected) {
                    void OnLobbyCreated(Socket _, Message response) {
                        if (response.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                            managedCallback?.Invoke(new ManagedLobbyStatus(true), new ManagedLobby(connection, true));
                        } else {
                            managedCallback?.Invoke(new ManagedLobbyStatus(false, response.Argument1), null);
                        }
                    }
                    MessageSender.SendMessage(connection.ConnectionSocket, new Message(Message_Type.LOBBY_CREATE, lobbyName, lobbyPassword), OnLobbyCreated);
                } else {
                    managedCallback?.Invoke(new ManagedLobbyStatus(false, "Unable to establish connection with server."), null);
                }
            }

            // Connect
            hub.Connect(OnConnectionEstablished);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="lobby"></param>
        /// <param name="password"></param>
        /// <param name="managedCallback"></param>
        public static void Join(LobbyHub hub, ConnectableLobby lobby, string password, ManagedLobbyConnectCallback managedCallback)
            => Join(hub, lobby.lobby_guid, password, managedCallback);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="lobbyGUID"></param>
        /// <param name="password"></param>
        /// <param name="managedCallback"></param>
        public static void Join(LobbyHub hub, string lobbyGUID, string password, ManagedLobbyConnectCallback managedCallback) {

            // Callback for establishing connection
            void OnConnectionEstablished(bool connected, Connection connection) {
                if (connected) {
                    void OnLobbyJoinResponse(Socket _, Message response) {
                        if (response.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                            managedCallback?.Invoke(new ManagedLobbyStatus(true), new ManagedLobby(connection, false));
                        } else {
                            managedCallback?.Invoke(new ManagedLobbyStatus(false, response.Argument1), null);
                        }
                    }
                    MessageSender.SendMessage(connection.ConnectionSocket, new Message(Message_Type.LOBBY_JOIN, lobbyGUID, password), OnLobbyJoinResponse);
                } else {
                    managedCallback?.Invoke(new ManagedLobbyStatus(false, "Unable to establish connection with server."), null);
                }
            }

            // Connect
            hub.Connect(OnConnectionEstablished);

        }

    }

}
