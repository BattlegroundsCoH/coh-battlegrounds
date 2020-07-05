using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class ManagedLobby {

        /// <summary>
        /// Send a file to all lobby members.
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
        public event ManagedLobbyQuery OnDataRequest;

        /// <summary>
        /// 
        /// </summary>
        public event ManagedLobbyFileReceived OnFileReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action OnStartMatchReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Func<string, object> OnLocalDataRequested;

        /// <summary>
        /// 
        /// </summary>
        public bool IsHost => m_isHost;

        private ManagedLobby(Connection connection, bool isHost) {

            // Assign the underlying connection and start listening for messages
            this.m_underlyingConnection = connection;
            this.m_underlyingConnection.OnMessage += this.ManagedLobbyInternal_MessageReceived;
            this.m_underlyingConnection.OnFile += x => this.OnFileReceived?.Invoke(x.Argument1, x.FileData != null, x.FileData);
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
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                Message queryMessage = new Message(Message_Type.LOBBY_INFO, lobbyInformation);
                Message.SetIdentifier(m_underlyingConnection.ConnectionSocket, queryMessage);
                void OnResponse(Message message) {
                    m_underlyingConnection.ClearIdentifierReceiver(message.Identifier);
                    reponse?.Invoke(message.Argument1, message.Argument2);
                }
                m_underlyingConnection.SetIdentifierReceiver(queryMessage.Identifier, OnResponse);
                m_underlyingConnection.SendMessage(queryMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyInformation"></param>
        /// <param name="lobbyInformationValue"></param>
        public void SetLobbyInformation(string lobbyInformation, string lobbyInformationValue) {
            if (m_isHost) {
                if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                    m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_UPDATE, lobbyInformation, lobbyInformationValue));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="filepath"></param>
        public int SendFile(string receiver, string filepath)
            => this.SendFile(receiver, filepath, -1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="filepath"></param>
        /// <param name="identifier"></param>
        public int SendFile(string receiver, string filepath, int identifier) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                return m_underlyingConnection.SendFile(receiver, filepath, identifier);
            } else {
                return -1;
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
                        if (from.CompareTo(SEND_ALL) != 0) {
                            m_underlyingConnection.ClearIdentifierReceiver(msg.Identifier);
                        }
                    } else {
                        response?.Invoke(from, false, null);
                    }
                }
                m_underlyingConnection.SetIdentifierReceiver(message.Identifier, OnMessage);
                m_underlyingConnection.SendMessage(message);
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

        private Company GetLocalCompany() {
            try {
                object val = this.OnLocalDataRequested?.Invoke("CompanyData");
                if (val is Company c) {
                    return c;
                } else if (val is string s) {
                    return Company.ReadCompanyFromFile(s);
                }
            } catch {}
            return null;
        }

        private async Task<(List<Company>, bool)> GetLobbyCompanies() {

            int expected = 1;
            DateTime start = DateTime.Now;
            List<byte[]> companyFiles = new List<byte[]>();

            void OnCompanyFileReceived(string from, bool wasReceived, byte[] filedata) {
                if (wasReceived) {
                    companyFiles.Add(filedata);
                }
            }

            this.GetCompanyFileFrom(SEND_ALL, OnCompanyFileReceived);

            while (companyFiles.Count != expected && (DateTime.Now - start).Minutes < 5) {
                await Task.Yield();
            }

            if ((DateTime.Now - start).Minutes >= 5) {
                return (null, false);
            }

            List<Company> companies = new List<Company>();

            foreach (byte[] companyData in companyFiles) {
                try {
                    companies.Add(Company.ReadCompanyFromBytes(companyData));
                } catch {
                    return (null, false);
                }
            }

            return (companies, true);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationCancelled"></param>
        public async void CompileAndStartMatch(Action<string> operationCancelled) {

            Company ownCompany = GetLocalCompany();
            if (ownCompany == null) {
                operationCancelled?.Invoke("Failed to load own company!");
                return;
            }

            (List<Company> lobbyCompanies, bool success) = await GetLobbyCompanies();

            if (success) {

                lobbyCompanies.Add(ownCompany);

                Company[] companies = lobbyCompanies.ToArray();

                Session session = null;

                try {

                    session = Session.CreateSession(
                        this.OnLocalDataRequested?.Invoke("Scenario") as string ?? string.Empty,
                        companies,
                        this.OnLocalDataRequested?.Invoke("Gamemode") as Wincondition ?? null,
                        (bool?)this.OnLocalDataRequested?.Invoke("EnableAI") ?? false);

                } catch (Exception e) {
                    operationCancelled?.Invoke(e.Message);
                }

                // Play the session
                SessionManager.PlaySession<SessionCompiler<CompanyCompiler>, CompanyCompiler>(
                    session,
                    this.ManagedLobbyInternal_GameSessionStatusChanged,
                    this.ManagedLobbyInternal_GameMatchAnalyzed,
                    async () => {
                        return await this.ManagedLobbyInternal_GameOnGamemodeCompiled(lobbyCompanies.Count, operationCancelled);
                    });

            } else {
                operationCancelled?.Invoke("Failed to get lobby companies");
            }

        }

        async Task<bool> ManagedLobbyInternal_GameOnGamemodeCompiled(int expected, Action<string> operationCancelled) {

            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

            if (File.Exists(sgapath)) {

                int confirmations = 0;

                // Send the message
                int confirmIdentifier = this.SendFile(SEND_ALL, sgapath);
                this.m_underlyingConnection.SetIdentifierReceiver(confirmIdentifier, x => {
                    if (x.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                        confirmations++;
                    } else {
                        operationCancelled?.Invoke("Failed to send .sga to one or more players!"); // TODO: Resend protocol
                    }
                });

                // Yield as long as we've not confirmed all cases.
                while (confirmations < expected) {
                    await Task.Yield();
                }

                // Send the start match...
                this.m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_STARTMATCH));

                return true;

            } else {
                operationCancelled?.Invoke("Failed to compile!");
            }

            return false;

        }

        void ManagedLobbyInternal_GameSessionStatusChanged(SessionStatus status, Session s) {
            // TODO: Implement
        }

        void ManagedLobbyInternal_GameMatchAnalyzed(GameMatch results) {
            // TODO: Sync
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
                    this.OnDataRequest?.Invoke(true, incomingMessage.Argument1, "CompanyData", incomingMessage.Identifier);
                    break;
                case Message_Type.LOBBY_REQUEST_RESULTS:
                    this.OnDataRequest?.Invoke(true, incomingMessage.Argument1, "MatchData", incomingMessage.Identifier);
                    break;
                case Message_Type.LOBBY_STARTMATCH:
                    this.OnStartMatchReceived?.Invoke();
                    break;
                case Message_Type.LOBBY_SENDFILE:

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
