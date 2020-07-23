using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

using Battlegrounds.Compiler;
using Battlegrounds.Functional;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

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
        /// The worker <see cref="Connection"/> used to connect to files. This is only available to the DLL.
        /// </summary>
        internal Connection WorkerConnection => this.m_underlyingConnection;

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
        public event ManagedLobbyMatchStart OnStartMatchReceived;

        /// <summary>
        /// Event triggered when the host has updated an information value in the lobby.
        /// </summary>
        public event ManagedLobbyInfoChanged OnLobbyInfoChanged;

        /// <summary>
        /// Function to solve local data requests. May return requested object or filepath to load object.
        /// </summary>
        public event ManagedLobbyLocalDataRequest OnLocalDataRequested;

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
        /// 
        /// </summary>
        /// <param name="response"></param>
        public void GetPlayersInLobby(Action<int> response) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                this.GetLobbyInformation("players", (a, b) => response.Invoke(int.Parse(a)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetPlayersInLobbyAsync() {
            int result = 0;
            bool done = false;
            this.GetPlayersInLobby(x => { result = x; done = true; });
            while (!done) {
                await Task.Delay(1);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        public void GetLobbyCapacity(Action<int> response) {
            if (m_underlyingConnection != null && m_underlyingConnection.IsConnected) {
                this.GetLobbyInformation("capacity", (a, b) => {
                    if (int.TryParse(a, out int c)) {
                        response.Invoke(c);
                    } else {
                        response.Invoke(-1);
                    }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetLobbyCapacityAsync() {
            int result = 0;
            bool done = false;
            this.GetLobbyCapacity(x => { result = x; done = true; });
            while (!done) {
                await Task.Delay(1);
            }
            return result;
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
                //m_underlyingConnection = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> GetPlayerNamesAsync() {

            string[] playernames = null;
            bool alldone = false;

            void OnMessage(Message response) {
                Trace.WriteLine(response);
                if (response.Descriptor == Message_Type.LOBBY_PLAYERNAMES) {
                    playernames = response.Argument1.Split(';', StringSplitOptions.RemoveEmptyEntries).ForEach(x => x.Replace("\"", ""));
                }
                alldone = true; // do this even if false...
            }

            Message playersQueryMessage = new Message(Message_Type.LOBBY_PLAYERNAMES);
            Message.SetIdentifier(m_underlyingConnection.ConnectionSocket, playersQueryMessage);
            m_underlyingConnection.SetIdentifierReceiver(playersQueryMessage.Identifier, OnMessage);
            m_underlyingConnection.SendMessage(playersQueryMessage);
            m_underlyingConnection.Listen(); // For some reason it just stops listening here...

            while (!alldone) {
                await Task.Delay(1);
            }

            return playernames;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetPlayerNames() 
            => this.GetPlayerNamesAsync().Result;

        private Company GetLocalCompany() {
            object val = this.OnLocalDataRequested?.Invoke("CompanyData");
            if (val is Company c) {
                return c;
            } else if (val is string s) {
                return Company.ReadCompanyFromFile(s);
            }
            return null;
        }

        private async Task<(List<Company>, bool)> GetLobbyCompanies() {

            FileSync sync = new FileSync(this, new Message(Message_Type.LOBBY_REQUEST_COMPANY, SEND_ALL));

            await sync.Sync();

            if (sync.IsSynced) {

                List<Company> companies = new List<Company>();

                foreach (FileSync.SyncFile file in sync.SyncedFiles) {
                    companies.Add(Company.ReadCompanyFromBytes(file.Data));
                }

                return (companies, true);

            } else {
                return (null, false);
            }

        }

        /// <summary>
        /// Compile the win condition using data from the lobby members and begin the match with all lobby members.<br/>This will start Company of Heroes 2 if completed.
        /// </summary>
        /// <remarks>The method is synchronous and make take several minutes to complete. (Use in a <see cref="Task.Run(Action)"/> context to maintain responsiveness).</remarks>
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

                Session session = null;

                try {

                    if (this.OnLocalDataRequested?.Invoke("MatchInfo") is SessionInfo info) {

                        // Zip/Map the companies to their respective SessionParticipant instances.
                        Session.ZipCompanies(lobbyCompanies.ToArray(), ref info);

                        // Create the session properly.
                        session = Session.CreateSession(info);

                    } else {
                        throw new Exception("Failed to get a valid SessionInfo");
                    }

                } catch (Exception e) {
                    operationCancelled?.Invoke(e.Message);
                }

                // Play the session
                SessionManager.PlaySession<SessionCompiler<CompanyCompiler>, CompanyCompiler>(
                    session,
                    this.ManagedLobbyInternal_GameSessionStatusChanged,
                    this.ManagedLobbyInternal_GameMatchAnalyzed,
                    async () => await this.ManagedLobbyInternal_GameOnGamemodeCompiled(operationCancelled));

            } else {
                operationCancelled?.Invoke("Failed to get lobby companies");
            }

        }

        async Task<bool> ManagedLobbyInternal_GameOnGamemodeCompiled(Action<string> operationCancelled) {

            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

            if (File.Exists(sgapath)) {

                FileSync sgaSync = new FileSync(this, sgapath);

                await sgaSync.Sync();

                if (sgaSync.SyncFailed) {
                    operationCancelled?.Invoke("Failed send .sga to all participants.");
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
            Trace.WriteLine($"Received: [{incomingMessage}]");
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
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.Kicked, incomingMessage.Argument1);
                    break;
                case Message_Type.LOBBY_SETHOST:
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.Host, string.Empty);
                    break;
                case Message_Type.LOBBY_INFO:
                    this.OnLobbyInfoChanged?.Invoke(incomingMessage.Argument1, incomingMessage.Argument2);
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
                case Message_Type.CONFIRMATION_MESSAGE:

                    break;
                default: Console.WriteLine(incomingMessage.Descriptor + ":" + incomingMessage.Argument1 + " [" + incomingMessage.Identifier + "]"); break;
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
        /// <exception cref="ArgumentException"/>
        public static void Host(LobbyHub hub, string lobbyName, string lobbyPassword, ManagedLobbyConnectCallback managedCallback) {

            // Make sure lobby name is valid
            if (string.IsNullOrEmpty(lobbyName)) {
                throw new ArgumentException("Must have a valid lobby name!");
            }

            // Just to be safe
            if (lobbyPassword == null) {
                lobbyPassword = string.Empty;
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
