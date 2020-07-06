using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// An abstracted representation of a lobby. This class cannot be inherited. This class has no public constructor.
    /// </summary>
    public sealed class ManagedLobby {

        /// <summary>
        /// Send a file to all lobby members.
        /// </summary>
        public const string SEND_ALL = "ALL";

        Connection m_underlyingConnection;
        bool m_isHost;

        /// <summary>
        /// Event triggered when a player-specific event was received.
        /// </summary>
        public event ManagedLobbyPlayerEvent OnPlayerEvent;

        /// <summary>
        /// Event triggered when a local client-side event was received.
        /// </summary>
        public event ManagedLobbyLocalEvent OnLocalEvent;

        /// <summary>
        /// Event triggered when the client receives a data request.
        /// </summary>
        public event ManagedLobbyQuery OnDataRequest;

        /// <summary>
        /// Even triggered when a file has been received.
        /// </summary>
        public event ManagedLobbyFileReceived OnFileReceived;

        /// <summary>
        /// Event triggered when the host has sent the <see cref="Message_Type.LOBBY_STARTMATCH"/> message.
        /// </summary>
        public event Action OnStartMatchReceived;

        /// <summary>
        /// Function to solve local data requests. May return requested object or filepath to load object.
        /// </summary>
        public event Func<string, object> OnLocalDataRequested;

        /// <summary>
        /// Is the instance of <see cref="ManagedLobby"/> considered to be the host of the lobby.
        /// </summary>
        public bool IsHost => m_isHost;

        private ManagedLobby(Connection connection, bool isHost) {

            // Assign the underlying connection and start listening for messages
            this.m_underlyingConnection = connection;
            this.m_underlyingConnection.OnMessage += this.ManagedLobbyInternal_MessageReceived;
            this.m_underlyingConnection.OnFile += this.ManagedLobbyInternal_FileReceived;
            this.m_underlyingConnection.Start();

            // Assign hostship
            this.m_isHost = isHost;

        }

        /// <summary>
        /// Send a chat message to the server.
        /// </summary>
        /// <param name="chatMessage">The contents of the chat message.</param>
        public void SendChatMessage(string chatMessage) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_CHATMESSAGE, chatMessage));
            }
        }

        /// <summary>
        /// Send a meta-message to all users in the server.
        /// </summary>
        /// <param name="metaMessage">The contents of the meta-message.</param>
        public void SendMetaMessage(string metaMessage) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_METAMESSAGE, metaMessage));
            }
        }

        /// <summary>
        /// Get a specific detail from the lobby.
        /// </summary>
        /// <param name="lobbyInformation">The information that is sought.</param>
        /// <param name="reponse">The query response callback to handle the server response.</param>
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
        /// Set a specific lobby detail. This will only be fully invoked if host.
        /// </summary>
        /// <param name="lobbyInformation">The information to change.</param>
        /// <param name="lobbyInformationValue">The new value to set.</param>
        public void SetLobbyInformation(string lobbyInformation, string lobbyInformationValue) {
            if (m_isHost) {
                if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                    m_underlyingConnection.SendMessage(new Message(Message_Type.LOBBY_UPDATE, lobbyInformation, lobbyInformationValue));
                }
            }
        }

        /// <summary>
        /// Send a file to another user in the lobby.
        /// </summary>
        /// <param name="receiver">The username of the recipient.</param>
        /// <param name="filepath">The filepath of the file to send.</param>
        /// <returns>The identifier used to send the file. -1 if unable to send file.</returns>
        public int SendFile(string receiver, string filepath)
            => this.SendFile(receiver, filepath, -1);

        /// <summary>
        /// Send a file to another user in the lobby with a specific identifier.
        /// </summary>
        /// <param name="receiver">The username of the recipient.</param>
        /// <param name="filepath">The filepath of the file to send.</param>
        /// <param name="identifier">The custom identifier to use when sending message.</param>
        /// <returns>The identifier used to send the file. -1 if unable to send file.</returns>
        public int SendFile(string receiver, string filepath, int identifier) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                return m_underlyingConnection.SendFile(receiver, filepath, identifier);
            } else {
                return -1;
            }
        }

        /// <summary>
        /// Get a random identifier to identify messages.
        /// </summary>
        /// <returns>A random identifier.</returns>
        public int GetRandomIdentifier()
            => Message.GetIdentifier(this.m_underlyingConnection.ConnectionSocket);

        /// <summary>
        /// Request the <see cref="Company"/> file from a specific user in the lobby.
        /// </summary>
        /// <param name="from">The user to request <see cref="Company"/> file from.</param>
        /// <param name="response">The <see cref="ManagedLobbyFileReceived"/> response callback to use when a response from the user is received.</param>
        public void GetCompanyFileFrom(string from, ManagedLobbyFileReceived response) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                Message message = new Message(Message_Type.LOBBY_REQUEST_COMPANY, from);
                Message.SetIdentifier(m_underlyingConnection.ConnectionSocket, message);
                void OnMessage(Message msg) {
                    if (msg.Descriptor == Message_Type.LOBBY_SENDFILE) {
                        response?.Invoke(msg.Argument2, msg.Argument1, true, msg.FileData, msg.Identifier);
                        if (from.CompareTo(SEND_ALL) != 0) {
                            m_underlyingConnection.ClearIdentifierReceiver(msg.Identifier);
                        }
                    } else {
                        response?.Invoke(from, null, false, null, msg.Identifier);
                    }
                }
                m_underlyingConnection.SetIdentifierReceiver(message.Identifier, OnMessage);
                m_underlyingConnection.SendMessage(message);
                Console.WriteLine("Sent request for getting company files.");
            }
        }

        /// <summary>
        /// Gracefully disconnect from the <see cref="ManagedLobby"/>.
        /// </summary>
        /// <remarks>The connection to the server will be broken and <see cref="Join(LobbyHub, string, string, ManagedLobbyConnectCallback)"/> must be used to reestablish connection.</remarks>
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
            int sendallIdentifier = -1;
            DateTime start = DateTime.Now;
            List<byte[]> companyFiles = new List<byte[]>();

            void OnCompanyFileReceived(string from, string filename, bool wasReceived, byte[] filedata, int identifier) {
                if (sendallIdentifier == -1) {
                    sendallIdentifier = identifier;
                }
                if (wasReceived) {
                    lock (companyFiles) {
                        companyFiles.Add(filedata);
                    }
                }
            }

            this.GetCompanyFileFrom(SEND_ALL, OnCompanyFileReceived);

            while (companyFiles.Count != expected && (DateTime.Now - start).Minutes < 5) {
                Console.WriteLine($"Waiting for files to be received [{companyFiles.Count}]");
                await Task.Delay(250);
            }

            if ((DateTime.Now - start).Minutes >= 5) {
                return (null, false);
            }

            // Clear the sendall identifier
            this.m_underlyingConnection.ClearIdentifierReceiver(sendallIdentifier);

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
        /// Compile the win condition using data from the lobby members and begin the match with all lobby members.<br/>This will start Company of Heroes 2 if completed.
        /// </summary>
        /// <remarks>The method is asynchronous and make take several minutes to complete.</remarks>
        /// <param name="operationCancelled">The <see cref="Action{T}"/> invoked if the execution of the method is cancelled. The <see cref="string"/> argument describes what caused the cancellation.</param>
        public async void CompileAndStartMatch(Action<string> operationCancelled) {

            // Make sure we're the host
            if (!m_isHost) {
                return;
            }

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
                        return await this.ManagedLobbyInternal_GameOnGamemodeCompiled(lobbyCompanies.Count - 1, operationCancelled);
                    });

            } else {
                operationCancelled?.Invoke("Failed to get lobby companies");
            }

        }

        async Task<bool> ManagedLobbyInternal_GameOnGamemodeCompiled(int expected, Action<string> operationCancelled) {

            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

            if (File.Exists(sgapath)) {

                int confirmations = 0;
                DateTime start = DateTime.Now;

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
                while (confirmations < expected && (DateTime.Now - start).Minutes < 5) {
                    Console.WriteLine($"Waiting for confirmations... [{confirmations}/{expected}]");
                    await Task.Delay(250);
                }

                if ((DateTime.Now - start).Minutes >= 5) {
                    return false;
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
            Console.WriteLine("Session update: " + status.ToString());
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
                    Console.WriteLine("got it here");
                    break;
                case Message_Type.CONFIRMATION_MESSAGE:
                    Console.WriteLine("Received OK here...");
                    break;
                default: Console.WriteLine(incomingMessage.Descriptor + ":" + incomingMessage.Identifier); break;
            }
        }

        private void ManagedLobbyInternal_FileReceived(Message incomingFileMessage) {
            this.OnFileReceived?.Invoke(incomingFileMessage.Argument2, incomingFileMessage.Argument1, incomingFileMessage.FileData != null, incomingFileMessage.FileData, incomingFileMessage.Identifier);
            if (incomingFileMessage.Argument1.CompareTo("coh2_battlegrounds_wincondition.sga") == 0) {
                Console.WriteLine("Received .sga -- returning OK message to " + incomingFileMessage.Argument2);
                this.m_underlyingConnection.SendMessage(
                    incomingFileMessage.CreateResponse(Message_Type.CONFIRMATION_MESSAGE, incomingFileMessage.Argument2, "Received .sga"));
            }
        }

        /// <summary>
        /// Host a new <see cref="ManagedLobby"/> on the central server.
        /// </summary>
        /// <param name="hub">The <see cref="LobbyHub"/> instance to use when attempting to host.</param>
        /// <param name="lobbyName">The name of the lobby.</param>
        /// <param name="lobbyPassword">The password of the lobby. Can be <see cref="string.Empty"/> if no password is desired.</param>
        /// <param name="managedCallback">The callback to invoke when the server has responded to the host request.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public static void Host(LobbyHub hub, string lobbyName, string lobbyPassword, ManagedLobbyConnectCallback managedCallback) {

            // Make sure lobby name is valid
            if (lobbyName == null) {
                throw new ArgumentNullException("Must have a valid lobby name!");
            }

            // Make sure it's not empty
            if (lobbyName.CompareTo(string.Empty) == 0) {
                throw new ArgumentException("Lobby name cannot be empty!");
            }

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
        /// Join an existing lobby in the <see cref="LobbyHub"/> using a <see cref="ConnectableLobby"/> to connect.
        /// </summary>
        /// <param name="hub">The <see cref="LobbyHub"/> instance to use when attempting to join.</param>
        /// <param name="lobby">The <see cref="ConnectableLobby"/> instance containing the GUID to use when attempting to join.</param>
        /// <param name="password">The password to send when trying to join.</param>
        /// <param name="managedCallback">The callback to invoke when the server has responded to the join request.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void Join(LobbyHub hub, ConnectableLobby lobby, string password, ManagedLobbyConnectCallback managedCallback)
            => Join(hub, lobby.lobby_guid, password, managedCallback);

        /// <summary>
        /// Join an existing lobby in the <see cref="LobbyHub"/>.
        /// </summary>
        /// <param name="hub">The <see cref="LobbyHub"/> instance to use when attempting to join.</param>
        /// <param name="lobby">The GUID to use when attempting to join.</param>
        /// <param name="password">The password to send when trying to join.</param>
        /// <param name="managedCallback">The callback to invoke when the server has responded to the join request.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static void Join(LobbyHub hub, string lobbyGUID, string password, ManagedLobbyConnectCallback managedCallback) {

            // Make sure we have a valid hub instance.
            if (hub == null) {
                throw new ArgumentNullException("The lobby hub instance was null!");
            }

            // Make sure the GUID is valid.
            if (lobbyGUID.Length != 36) {
                throw new ArgumentOutOfRangeException("The GUID was not of length 36 and is therefore not a valid GUID.");
            }

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
