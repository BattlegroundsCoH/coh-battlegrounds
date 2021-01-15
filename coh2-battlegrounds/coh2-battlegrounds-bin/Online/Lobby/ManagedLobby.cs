using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Battlegrounds.Compiler;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Online.Debug;
using Battlegrounds.Online.Services;
using Battlegrounds.Verification;
using Battlegrounds.Steam;

namespace Battlegrounds.Online.Lobby {

    /// <summary>
    /// Callback for handlings events of a single <see cref="int"/> argument.
    /// </summary>
    /// <param name="argument">The <see cref="int"/> argument given to the callback.</param>
    public delegate void IntegerCallback(int argument);

    /// <summary>
    /// Callback for handlings events of a single <see cref="ulong"/> argument.
    /// </summary>
    /// <param name="argument">The <see cref="ulong"/> argument given to the callback.</param>
    public delegate void UlongCallback(ulong argument);

    /// <summary>
    /// A representation of a clientside lobby that handles communication to and from the Battlegrounds server. This class cannot be inherited. This class has no public constructor.
    /// </summary>
    /// <remarks>
    /// See <see cref="Join(LobbyHub, string, string, ManagedLobbyConnectCallback)"/> or <see cref="Host(LobbyHub, string, string, ManagedLobbyConnectCallback)"/> to create a new instance.
    /// </remarks>
    public sealed class ManagedLobby {

        /// <summary>
        /// Send a file to all lobby members.
        /// </summary>
        public const string SEND_ALL = "ALL";

        private SteamUser m_self;
        private Connection m_underlyingConnection;
        private string m_lobbyID;
        private string m_lobbyMap;
        private string m_lobbyGamemode;
        private string m_lobbyGamemodeOption;
        private bool m_isHost;
        private Dictionary<ManagedLobbyTeamType, ManagedLobbyTeam> m_teams;

        /// <summary>
        /// The file ID on the file server
        /// </summary>
        public string LobbyFileID => this.m_lobbyID.Replace("-", "");

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
        /// Event triggered when the host has sent the <see cref="MessageType.LOBBY_STARTMATCH"/> message.
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
        /// Event triggered when new informaion regarding the starting match has been received.
        /// </summary>
        public event ManagedLobbyMatchInfo OnMatchInfoReceived;

        /// <summary>
        /// Is the instance of <see cref="ManagedLobby"/> considered to be the host of the lobby.
        /// </summary>
        public bool IsHost => this.m_isHost;

        /// <summary>
        /// The <see cref="SteamUser"/> that's connected to the server. (The local user).
        /// </summary>
        public SteamUser Self => this.m_self;

        /// <summary>
        /// Is the underlying connection connected to the server.
        /// </summary>
        public bool IsConnectedToServer => this.m_underlyingConnection.IsConnected;

        /// <summary>
        /// The current player count
        /// </summary>
        public int PlayerCount => this.m_teams[ManagedLobbyTeamType.Allies].Count + this.m_teams[ManagedLobbyTeamType.Axis].Count;

        /// <summary>
        /// The currently selected map
        /// </summary>
        public string SelectedMap => this.m_lobbyMap;

        /// <summary>
        /// Gets the currently registered to be selected gamemode.
        /// </summary>
        public string SelectedGamemode => this.m_lobbyGamemode;

        /// <summary>
        /// Gets the currently registered to be selected gamemode.
        /// </summary>
        public string SelectedGamemodeOption => this.m_lobbyGamemodeOption;

        private List<ManagedLobbyTeam> Teams => this.m_teams.Values.ToList();

        private ManagedLobby(Connection connection, bool isHost) {

            // Assign the underlying connection and start listening for messages
            this.m_underlyingConnection = connection;
            this.m_underlyingConnection.OnMessage += this.ManagedLobbyInternal_MessageReceived;
            this.m_underlyingConnection.Start();

            // Assign hostship
            this.m_isHost = isHost;

            // Create teams
            this.m_teams = new Dictionary<ManagedLobbyTeamType, ManagedLobbyTeam>() {
                [ManagedLobbyTeamType.Allies] = new ManagedLobbyTeam(this, 1, ManagedLobbyTeamType.Allies),
                [ManagedLobbyTeamType.Axis] = new ManagedLobbyTeam(this, 1, ManagedLobbyTeamType.Axis),
                [ManagedLobbyTeamType.Spectator] = new ManagedLobbyTeam(this, 4, ManagedLobbyTeamType.Spectator),
            };

        }

        /// <summary>
        /// Send a chat message to the server.
        /// </summary>
        /// <param name="chatMessage">The contents of the chat message.</param>
        public void SendChatMessage(string chatMessage) {
            if (this.m_underlyingConnection != null && this.m_underlyingConnection.IsConnected) {
                this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_CHATMESSAGE, chatMessage));
            }
        }

        /// <summary>
        /// Send a meta-message to all users in the server.
        /// </summary>
        /// <param name="metaMessage">The contents of the meta-message.</param>
        public void SendMetaMessage(string metaMessage) {
            if (this.m_underlyingConnection != null && this.m_underlyingConnection.IsConnected) {
                this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_METAMESSAGE, metaMessage));
            }
        }

        /// <summary>
        /// Get the amount of active players in the lobby.
        /// </summary>
        /// <param name="response">The callback function that will received the server response. If connection fails, -1 is returned.</param>
        /// <returns>Will return <see langword="true"/> if the callback was valid. Otherwise <see langword="false"/>.</returns>
        public bool GetPlayersInLobby(IntegerCallback response) {
            if (response is null) {
                return false; // Bail-fast --> no need to request data from the server if we're not going to use it.
            }
            if (this.m_underlyingConnection != null && this.m_underlyingConnection.IsConnected) {
                this.GetLobbyInformation("players", (a, b) => response.Invoke(int.Parse(a)));
                return true;
            } else {
                response.Invoke(-1);
                return false;
            }
        }

        /// <summary>
        /// Get the amount of active players in the lobby through an asynchronous operation
        /// </summary>
        /// <remarks>Async method.</remarks>
        /// <returns>The integer response from the server. If connection fails, -1 is returned.</returns>
        public async Task<int> GetPlayersInLobbyAsync() {
            int result = -1;
            bool done = false;
            if (this.GetPlayersInLobby(x => { result = x; done = true; })) {
                while (!done) {
                    await Task.Delay(1);
                }
            }
            return result;
        }

        /// <summary>
        /// Set the currently selected <see cref="Scenario"/> in the <see cref="ManagedLobby"/>.
        /// </summary>
        /// <param name="scenario">The <see cref="Scenario"/> to set as selected map.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetMap(Scenario scenario) {
            if (scenario is null) {
                return;
            }
            if (!this.m_isHost) {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            } else {
                this.m_lobbyMap = scenario.RelativeFilename;
                this.SetLobbyInformation("selected_map", this.m_lobbyMap);
            }
        }

        /// <summary>
        /// Set the currently selected gamemode.
        /// </summary>
        /// <param name="gamemode">The name of the gamemode to set.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetGamemode(string gamemode) {
            if (gamemode is null) {
                return;
            }
            if (!this.m_isHost) {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            } else {
                this.m_lobbyGamemode = gamemode;
                this.SetLobbyInformation("selected_wc", this.m_lobbyGamemode);
            }
        }

        /// <summary>
        /// Set the currently selected option for the selected gamemode.
        /// </summary>
        /// <param name="option">The numeric ID of the option to set.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetGamemodeOption(int option) {
            if (!this.m_isHost) {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            } else {
                this.m_lobbyGamemodeOption = option.ToString();
                this.SetLobbyInformation("selected_wcs", this.m_lobbyGamemodeOption);
            }
        }

        /// <summary>
        /// Set the faction of the specified user.
        /// </summary>
        /// <param name="ID">The unique ID of the user to set faction for.</param>
        /// <param name="faction">The AE name of the faction to set.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetFaction(ulong ID, string faction) {
            if (this.m_isHost || ID == this.m_self.ID) {
                this.SetUserInformation(ID, "fac", faction);
            } else {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            }
            if (this.TryFindPlayerFromID(ID) is ManagedLobbyMember member) {
                member.UpdateFaction(faction);
            }
        }

        /// <summary>
        /// Set the <see cref="Company"/> of the local user.
        /// </summary>
        /// <param name="company">The actual <see cref="Company"/> to set as selected company.</param>
        /// <param name="upload">Upload the selected company file to the server. (Set to <see langword="true"/> only when absolutely needed).</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetCompany(Company company, bool upload = false) {
            this.SetCompany(this.Self.ID, company.Name, company.GetStrength());
            upload.Then(() => this.UploadCompany(company));
        }

        /// <summary>
        /// Set the name and strength of the selected company for a specific user.
        /// </summary>
        /// <param name="ID">The unique ID of the user to set company data for.</param>
        /// <param name="company">The display name of the company.</param>
        /// <param name="strength">The strength associated with the company.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetCompany(ulong ID, string company, double strength) {
            if (this.m_isHost || ID == this.m_self.ID) {
                this.SetUserInformation(ID, "com", company);
                this.SetUserInformation(ID, "str", strength);
            } else {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            }
            if (this.TryFindPlayerFromID(ID) is ManagedLobbyMember member) {
                member.UpdateCompany(company, strength);
            }
        }

        /// <summary>
        /// Get the current capacity of the lobby. -1 is returned if no valid value could be fetched from the server.
        /// </summary>
        /// <param name="response">The integer callback to handle the server response.</param>
        /// <returns>Will return <see langword="true"/> if the callback was valid. Otherwise <see langword="false"/>.</returns>
        public bool GetLobbyCapacity(IntegerCallback response) {
            if (response is null) {
                return false; // Bail-fast --> no need to request data from the server if we're not going to use it.
            }
            if (this.m_underlyingConnection != null && this.m_underlyingConnection.IsConnected) {
                this.GetLobbyInformation("capacity", (a, b) => {
                    if (int.TryParse(a, out int c)) {
                        response.Invoke(c);
                    } else {
                        response.Invoke(-1);
                    }
                });
                return true;
            } else {
                response.Invoke(-1);
                return false;
            }
        }

        /// <summary>
        /// Get the current capacity of the lobby.
        /// </summary>
        /// <remarks>Async method.</remarks>
        /// <returns>The response from the server. -1 is returned if no valid value could be fetched from the server.</returns>
        public async Task<int> GetLobbyCapacityAsync() {
            int result = -1;
            bool done = false;
            if (this.GetLobbyCapacity(x => { result = x; done = true; })) {
                while (!done) {
                    await Task.Delay(1);
                }
            }
            return result;
        }

        /// <summary>
        /// Get a specific detail from the lobby.
        /// </summary>
        /// <param name="lobbyInformation">The information that is sought.</param>
        /// <param name="response">The query response callback to handle the server response. If connection fails, both arguments will be <see cref="string.Empty"/>.</param>
        /// <returns>Will return <see langword="true"/> if the callback was valid. Otherwise <see langword="false"/>.</returns>
        public bool GetLobbyInformation(string lobbyInformation, ManagedLobbyQueryResponse response) {
            if (response is null) {
                return false; // Bail-fast --> no need to request data from the server if we're not going to use it.
            }
            if (this.m_underlyingConnection != null && this.m_underlyingConnection.IsConnected) {
                Message queryMessage = new Message(MessageType.LOBBY_INFO, lobbyInformation);
                Message.SetIdentifier(this.m_underlyingConnection.ConnectionSocket, queryMessage);
                void OnResponse(Message message) {
                    this.m_underlyingConnection.ClearIdentifierReceiver(message.Identifier);
                    response?.Invoke(message.Argument1, message.Argument2);
                }
                this.m_underlyingConnection.SetIdentifierReceiver(queryMessage.Identifier, OnResponse);
                this.m_underlyingConnection.SendMessage(queryMessage);
                return true;
            } else {
                response?.Invoke(string.Empty, string.Empty);
                return false;
            }
        }

        /// <summary>
        /// Get a specific detail from the lobby.
        /// </summary>
        /// <param name="lobbyInformation">The information that is sought from the server.</param>
        /// <returns>The first argument of the received server response.</returns>
        /// <remarks>Async method.</remarks>
        public async Task<string> GetLobbyInformation(string lobbyInformation) {
            string response = null;
            if (this.GetLobbyInformation(lobbyInformation, (a, b) => response = a)) {
                while (response is null) {
                    await Task.Delay(1);
                }
            }
            return response;
        }

        /// <summary>
        /// Get a specific user-related information detail from the server.
        /// </summary>
        /// <param name="userInformation">The specific information that is sought from the server.</param>
        /// <param name="response">The <see cref="ManagedLobbyQueryResponse"/> to handle the server response. Both arguments may be <see cref="string.Empty"/> if no response from server could be received.</param>
        /// <param name="userIndex">The unique ID of the user the information is sought for. By default set as the local user.</param>
        /// <returns>Will return <see langword="true"/> if the callback was valid. Otherwise <see langword="false"/>.</returns>
        public bool GetUserInformation(string userInformation, ManagedLobbyQueryResponse response, ulong userIndex = ulong.MaxValue) {
            if (response is null) {
                return false; // Bail-fast --> no need to request data from the server if we're not going to use it.
            }
            if (this.m_underlyingConnection != null && this.m_underlyingConnection.IsConnected) {
                Message queryMessage = null;
                if (userIndex == ulong.MaxValue) { // If default value, we're just looking for information of the local user (which the server can identify).
                    queryMessage = new Message(MessageType.LOBBY_GETUSERDATA, userInformation);
                } else {
                    queryMessage = new Message(MessageType.LOBBY_GETUSERDATA, userIndex.ToString(), userInformation);
                }
                Message.SetIdentifier(this.m_underlyingConnection.ConnectionSocket, queryMessage);
                void OnResponse(Message message) {
                    this.m_underlyingConnection.ClearIdentifierReceiver(message.Identifier);
                    response?.Invoke(message.Argument1, message.Argument2);
                }
                this.m_underlyingConnection.SetIdentifierReceiver(queryMessage.Identifier, OnResponse);
                this.m_underlyingConnection.SendMessage(queryMessage);
                return true;
            } else {
                response.Invoke(string.Empty, string.Empty);
                return false;
            }
        }

        /// <summary>
        /// Get a specific detail from the lobby.
        /// </summary>
        /// <param name="userInfo">The information that is sought from the server.</param>
        /// <returns>The first argument of the received server response.</returns>
        /// <remarks>Async method.</remarks>
        public async Task<string> GetUserInformation(string userInfo) {
            string response = null;
            if (this.GetUserInformation(userInfo, (a, _) => response = a)) {
                while (response is null) {
                    await Task.Delay(1);
                }
            }
            return response;
        }

        /// <summary>
        /// Get a specific detail from the lobby.
        /// </summary>
        /// <param name="lobbyInformation">The information that is sought from the server.</param>
        /// <returns>The first argument of the received server response.</returns>
        /// <remarks>Async method.</remarks>
        public async Task<string> GetUserInformation(ulong lobbyUser, string userInfo) {
            string response = null;
            if (this.GetUserInformation(userInfo, (a, _) => response = a, lobbyUser)) {
                while (response is null) {
                    await Task.Delay(1);
                }
            }
            return response;
        }

        /// <summary>
        /// Set a specific lobby detail. This will only be fully invoked if host.
        /// </summary>
        /// <param name="lobbyInformation">The information to change.</param>
        /// <param name="lobbyInformationValue">The new value to set.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetLobbyInformation(string lobbyInformation, object lobbyInformationValue) {
            if (this.m_isHost) {
                if (lobbyInformationValue is null) {
                    lobbyInformationValue = string.Empty;
                }
                if (this.m_underlyingConnection is not null && this.m_underlyingConnection.IsConnected) {
                    this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_UPDATE, lobbyInformation, lobbyInformationValue.ToString()));
                }
            } else {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            }
        }

        /// <summary>
        /// Update user-specific information for specified lobby member. Will require hostship to apply.
        /// </summary>
        /// <param name="lobbyIndex">The index of the user in lobby to update value of.</param>
        /// <param name="userinfo">The user info to change (Like, fac, tid, or com)</param>
        /// <param name="uservalue">The value to update information to.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetUserInformation(ulong lobbyIndex, string userinfo, object uservalue) {
            if (this.m_isHost) {
                if (uservalue is null) {
                    uservalue = string.Empty;
                }
                if (this.m_underlyingConnection is not null && this.m_underlyingConnection.IsConnected) {
                    this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_SETUSERDATA, lobbyIndex.ToString(), userinfo, uservalue.ToString()));
                }
            } else if (lobbyIndex == this.m_self.ID) {
                this.SetUserInformation(userinfo, uservalue);
            } else {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            }
        }

        /// <summary>
        /// Update user-specific information for local user (doesn't require hostship)
        /// </summary>
        /// <param name="userinfo">The user info to change (Like, fac, tid, or com)</param>
        /// <param name="uservalue">The value to update information to.</param>
        public void SetUserInformation(string userinfo, object uservalue) {
            if (uservalue is null) {
                uservalue = string.Empty;
            }
            if (this.m_underlyingConnection is not null && this.m_underlyingConnection.IsConnected) {
                this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_SETUSERDATA, userinfo, uservalue.ToString()));
            }
        }

        /// <summary>
        /// Set the max number of players that can join the lobby. Each playable team gains half of the specified amount. May only be invoked by host.
        /// </summary>
        /// <param name="capacity">The maximum amount of players that are allowed to join.</param>
        /// <param name="broadcast">Should broadcast the change in capacity</param>
        /// <exception cref="PermissionDeniedException"/>
        public void SetLobbyCapacity(int capacity, bool broadcast = true) {
            if (this.m_isHost) {
                int teamSize = capacity / 2;
                var alliesRemoved = this.m_teams[ManagedLobbyTeamType.Allies].SetCapacity(teamSize);
                var totalRemoved = this.m_teams[ManagedLobbyTeamType.Axis].SetCapacity(teamSize).Union(alliesRemoved);
                foreach (ManagedLobbyMember member in totalRemoved) {
                    if (member is AILobbyMember) {
                        this.RemoveAI(member.ID);
                    } else {
                        if (!this.m_teams[ManagedLobbyTeamType.Spectator].Join(member)) {
                            // TODO: Handle
                        }
                    }
                }
                broadcast.Then(() => this.m_isHost.Then(
                        () => this.SetLobbyInformation("capacity", capacity)).Else(
                        () => throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY))
                    );
            }
        }

        /// <summary>
        /// Get a random identifier to identify messages.
        /// </summary>
        /// <returns>A random identifier.</returns>
        public int GetRandomIdentifier() => Message.GetIdentifier(this.m_underlyingConnection.ConnectionSocket);

        /// <summary>
        /// Gracefully disconnect from the <see cref="ManagedLobby"/>.
        /// </summary>
        /// <remarks>The connection to the server will be broken and <see cref="Join(LobbyHub, string, string, ManagedLobbyConnectCallback)"/> must be used to reestablish connection.</remarks>
        public void Leave() {
            if (this.m_underlyingConnection != null) {
                this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_LEAVE));
                this.m_underlyingConnection.Stop();
            }
        }

        /// <summary>
        /// Get an array of the names of all active players in the lobby.
        /// </summary>
        /// <remarks>Async method.</remarks>
        /// <returns>Array of name for all members in the lobby. If any error, <see langword="null"/> is returned.</returns>
        public async Task<string[]> GetPlayerNamesAsync() {

            string[] playernames = null;
            bool alldone = false;

            void OnMessage(Message response) {
                Trace.WriteLine(response, "ManagedLobby");
                if (response.Descriptor == MessageType.LOBBY_PLAYERNAMES) {
                    playernames = response.Argument1.Split(';', StringSplitOptions.RemoveEmptyEntries).ForEach(x => x.Replace("\"", ""));
                }
                alldone = true; // do this even if false...
            }

            Message playersQueryMessage = new Message(MessageType.LOBBY_PLAYERNAMES);
            Message.SetIdentifier(this.m_underlyingConnection.ConnectionSocket, playersQueryMessage);
            this.m_underlyingConnection.SetIdentifierReceiver(playersQueryMessage.Identifier, OnMessage);
            this.m_underlyingConnection.SendMessage(playersQueryMessage);
            this.m_underlyingConnection.Listen(); // For some reason it just stops listening here...

            while (!alldone) {
                await Task.Delay(1);
            }

            return playernames;

        }

        /// <summary>
        /// Get an array of the IDs of all active players in the lobby.
        /// </summary>
        /// <remarks>Async method.</remarks>
        /// <returns>Array of IDs for all members in the lobby. If any error, <see langword="null"/> is returned.</returns>
        public async Task<ulong[]> GetPlayerIDsAsync() {

            ulong[] playerids = null;
            bool alldone = false;

            void OnMessage(Message response) {
                Trace.WriteLine(response, "ManagedLobby");
                if (response.Descriptor == MessageType.LOBBY_PLAYERIDS) {
                    playerids = response.Argument1.Split(';', StringSplitOptions.RemoveEmptyEntries).ForEach(x => x.Replace("\"", "")).Select(x => ulong.Parse(x)).ToArray();
                }
                alldone = true;
            }

            Message playerIDQueryMessage = new Message(MessageType.LOBBY_PLAYERIDS);
            Message.SetIdentifier(this.m_underlyingConnection.ConnectionSocket, playerIDQueryMessage);
            this.m_underlyingConnection.SetIdentifierReceiver(playerIDQueryMessage.Identifier, OnMessage);
            this.m_underlyingConnection.SendMessage(playerIDQueryMessage);
            this.m_underlyingConnection.Listen();

            while (!alldone) {
                await Task.Delay(1);
            }

            return playerids;

        }

        /// <summary>
        /// Retrieve the ID of the first lobby member with matching name.
        /// </summary>
        /// <param name="player">The string name of the user to find ID of.</param>
        /// <remarks>Async method.Note: This method may return incorrect ID if two users share the same name.</remarks>
        /// <returns>The found unique ID. if any error occurs, <see cref="ulong.MaxValue"/> is returned.</returns>
        public async Task<ulong> GetLobbyPlayerIDAsync(string player) {

            ulong id = ulong.MaxValue;
            bool alldone = false;

            void OnMessage(Message message) {
                Trace.WriteLine(message, "ManagedLobby");
                if (message.Descriptor == MessageType.LOBBY_GETPLAYERID) {
                    _ = ulong.TryParse(message.Argument1, out id);
                }
                alldone = true;
            }

            Message idQueryMessage = new Message(MessageType.LOBBY_GETPLAYERID, player);
            Message.SetIdentifier(this.m_underlyingConnection.ConnectionSocket, idQueryMessage);
            this.m_underlyingConnection.SetIdentifierReceiver(idQueryMessage.Identifier, OnMessage);
            this.m_underlyingConnection.SendMessage(idQueryMessage);
            this.m_underlyingConnection.Listen();

            while (!alldone) {
                await Task.Delay(1);
            }

            return id;

        }

        /// <summary>
        /// Get an array of the difficulty numberic values of all designated players in the lobby.
        /// </summary>
        /// <param name="players">The unique ID of the players the difficulty is sought after.</param>
        /// <remarks>Async method.</remarks>
        /// <returns>Array of numeric values of difficulty levels for all requested members.</returns>
        public async Task<int[]> GetLobbyPlayerDifficultiesAsync(ulong[] players) {

            int[] diffs = new int[players.Length];
            for (int i = 0; i < players.Length; i++) {
                string diff = await this.GetUserInformation(players[i], "dif");
                if (int.TryParse(diff, out int d)) {
                    diffs[i] = d;
                }
            }

            return diffs;

        }

        private Company GetLocalCompany() {
            object val = this.OnLocalDataRequested?.Invoke("CompanyData"); // Invoke the local data requester.
            if (val is Company c) { // If company is valid then return it
                return c;
            } else if (val is string s) { // if a string, try and read from that
                if (File.Exists(s)) {
                    try {
                        return Company.ReadCompanyFromFile(s);
                    } catch (ChecksumViolationException sumViolation) {
                        Trace.WriteLine($"Failed to retrieve local company [Checksum-Violation]; {sumViolation}", "ManagedLobby");
                    }
                }
            }
            return null; // return null otherwise.
        }

        /// <summary>
        /// Upload the company file that is found at the specified file path.
        /// </summary>
        /// <param name="companyFile">The path to the company file to upload.</param>
        /// <returns>If file was successfully uploaded, <see langword="true"/> is returned. Otherwise <see langword="false"/> is returned, if file didn't exist or upload failed.</returns>
        public bool UploadCompany(string companyFile) {

            // Make sure the file exists
            if (File.Exists(companyFile)) {

                // Read the company
                Company company = Company.ReadCompanyFromFile(companyFile);
                company.Owner = this.m_self.ID.ToString();

                // Upload the company and handle potential errors.
                return this.UploadCompany(company);

            } else {
                
                // Return false ==> File didn't exist
                return false;

            }

        }

        /// <summary>
        /// Upload the company instance to the file server.
        /// </summary>
        /// <param name="company">The company instance to upload.</param>
        /// <returns>If file was successfully uploaded, <see langword="true"/> is returned. Otherwise <see langword="false"/> is returned.</returns>
        public bool UploadCompany(Company company) {
            if (!FileHub.UploadFile(company.ToBytes(), $"{this.m_self.ID}_company.json", this.LobbyFileID)) {
                Trace.WriteLine("Failed to upload company...", "ManagedLobby");
                // ... do something?
                return false;
            }
            return true;
        }

        /// <summary>
        /// Download the uploaded <see cref="Company"/> instance from the specified user.
        /// </summary>
        /// <param name="userID">The unique ID of the user to download company from.</param>
        /// <param name="destination">The output destination to save the downloaded <see cref="Company"/> file from.</param>
        /// <returns>If download was successful <see langword="true"/> is returned. Otherwise <see langword="false"/> is returned.</returns>
        public bool GetLobbyCompany(ulong userID, string destination) 
            => FileHub.DownloadFile(destination, $"{userID}_company.json", this.LobbyFileID);
        
        private async Task<(List<Company>, bool)> GetLobbyCompanies() {

            // Initialize variables
            List<Company> companies = new List<Company>();
            bool success = false;

            // Calculate amount of human players (exluding the host).
            int humanCount = -1;
            this.Teams.ForEach(x => x.ForEachMember(y => humanCount += y is HumanLobbyMember ? 1 : 0));

            // Download all asynchronously.
            await Task.Run(async () => {

                // Keep an attempts counter
                int attempts = 0;

                // Keep track of members from which it was possible to find company files.
                HashSet<HumanLobbyMember> members = new HashSet<HumanLobbyMember>();

                // While not all human companies have been downloaded and attempts are still below 100
                while (members.Count != humanCount && attempts < 100) {

                    // Go through each team and its members
                    this.Teams.ForEach(x => {
                        x.ForEachMember(y => {
                            if (!members.Contains(y)) { // If we've not yet downloaded this members company
                                if (y is HumanLobbyMember && y.ID != this.Self.ID) { // and this member is a human and not self.
                                    // Define destination
                                    string destination = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, $"{y.ID}_company.json");
                                    // Try and download the company
                                    if (this.GetLobbyCompany(y.ID, destination)) {
                                        Company company = Company.ReadCompanyFromFile(destination); // Read in the company file and add to list.
                                        company.Owner = y.ID.ToString();
                                        companies.Add(company);
                                        members.Add(y as HumanLobbyMember); // Register member in downloaded set.
                                        Trace.WriteLine($"Downloaded company for user {y.ID} ({y.Name}) titled '{company.Name}'");
                                    } else { // If fail, log and try again in 100ms
                                        Trace.WriteLine($"Failed to download company for use {y.ID}. Will attempt again in 100ms");
                                        // TODO: Send message to use it wasn't possible to download their file.
                                    }
                                }
                            }
                        });
                    });

                    // Wait 100ms to give users a chance to uploaded
                    await Task.Delay(100);

                    // Increase attempts counter
                    attempts++;

                }

                // Did we get all members?
                success = members.Count == humanCount;

                // Log
                Trace.WriteLine($"Found {members.Count} and expected {humanCount}, following {attempts} attempts, thus retrieving companies will return {success}");

            });

            // Return downloaded companies and if we got all
            return (companies, success);

        }

        /// <summary>
        /// Retrieve the <see cref="ManagedLobbyTeam"/> that belongs to the specified <see cref="ManagedLobbyTeamType"/>.
        /// </summary>
        /// <param name="type">The type of team to find.</param>
        /// <returns>The <see cref="ManagedLobbyTeam"/> with specified type.</returns>
        public ManagedLobbyTeam GetTeam(ManagedLobbyTeamType type) => this.m_teams[type];

        /// <summary>
        /// Refresh the teams in an asynchronous manner.
        /// </summary>
        /// <remarks>Async method.</remarks>
        /// <param name="taskDone">Callback function called when the refresh is done.</param>
        public async void RefreshTeamAsync(ManagedLobbyTaskDone taskDone) {

            // Get all player IDs
            ulong[] playerIDs = await this.GetPlayerIDsAsync();

            // Log
            Trace.WriteLine($"PlayerIDs: {string.Join(',', playerIDs)}", "ManagedLobby.Refresh");

            // Get the capacity
            int capacity = await this.GetLobbyCapacityAsync();
            int tSize = capacity / 2;

            // Log
            Trace.WriteLine($"Capacity: {capacity}, team capacities: {tSize}", "ManagedLobby.Refresh");

            // Clear teams
            for (int i = 0; i < ManagedLobbyTeam.TeamTypes.Length; i++) {
                this.m_teams[ManagedLobbyTeam.TeamTypes[i]].Clear();
                this.m_teams[ManagedLobbyTeam.TeamTypes[i]].SetCapacity(tSize);
            }

            //Loop through all players
            for (int i = 0; i < playerIDs.Length; i++) {

                // Find out if it's a human or AI
                AIDifficulty diff = AIDifficulty.Human;
                if (int.TryParse(await this.GetUserInformation(playerIDs[i], "dif"), out int d)) {
                    diff = (AIDifficulty)d;
                }

                // Is human?
                if (diff == AIDifficulty.Human) {

                    // Get member name and create them as human
                    string name = await this.GetUserInformation(playerIDs[i], "name");
                    string faction = await this.GetUserInformation(playerIDs[i], "fac");
                    string company = await this.GetUserInformation(playerIDs[i], "com");
                    if (!double.TryParse(await this.GetUserInformation(playerIDs[i], "str"), out double strength)) {
                        Trace.WriteLine("Failed to convert strength", "ManagedLobby.Refresh");
                    } else {
                        strength = -1.0;
                    }

                    // Create human lobby member and update their faction
                    HumanLobbyMember human = new HumanLobbyMember(this, playerIDs[i], name, company, strength);
                    human.UpdateFaction(faction);

                    // Find team from faction
                    ManagedLobbyTeamType team = ManagedLobbyTeam.GetTeamTypeFromFaction(faction);

                    // Find index on team
                    int pos = int.Parse(await this.GetUserInformation(playerIDs[i], "pos"));

                    // Add human
                    this.GetTeam(team).Join(human);
                    this.GetTeam(team).TrySetMemberPosition(human, pos);

                    // Log
                    Trace.WriteLine($"Found human player: {name}, playing as {faction} for team {team} with pos {pos}", "ManagedLobby.Refresh");

                } else {

                    // Create AI
                    AILobbyMember ai = new AILobbyMember(this, diff, await this.GetUserInformation(playerIDs[i], "fac"), playerIDs[i]);

                    // Find team from faction
                    var team = ManagedLobbyTeam.GetTeamTypeFromFaction(ai.Faction);

                    // Find index on team
                    int pos = int.Parse(await this.GetUserInformation(playerIDs[i], "pos"));

                    // Add AI
                    this.GetTeam(team).Join(ai);
                    this.GetTeam(team).TrySetMemberPosition(ai, pos);

                    // Log
                    Trace.WriteLine($"Found AI player playing as {ai.Faction} for {team} at {pos}.", "ManagedLobby.Refresh");

                }

                // Add in some artificial delay (Should then make it easier for server/client communication).
                await Task.Delay(150);

            }

            // Call the task done
            taskDone?.Invoke(this);

        }

        /// <summary>
        /// Try and create a new <see cref="AILobbyMember"/> in the <see cref="ManagedLobby"/>. This will not create the <see cref="AILobbyMember"/> until the <see cref="MessageType.LOBBY_AIJOIN"/> message is received.
        /// </summary>
        /// <param name="difficulty">The difficulty of the AI.</param>
        /// <param name="faction">The faction of the AI.</param>
        /// <param name="teamIndex">The index (position) of the AI on the given team.</param>
        /// <param name="timeout">The amount of time to wait before timing out the attempt to create AI player. Default is -1.</param>
        /// <returns>The ID given to the <see cref="AILobbyMember"/> by the server.</returns>
        /// <remarks>Async method.</remarks>
        /// <exception cref="PermissionDeniedException"/>
        public async Task<int> TryCreateAIPlayer(AIDifficulty difficulty, string faction, int teamIndex, int timeout = -1) {
            if (!this.m_isHost) {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            }
            string responseQuery = null;
            Message addAIMessage = new Message(MessageType.LOBBY_ADDAI, ((int)difficulty).ToString(), faction, teamIndex.ToString());
            Message.SetIdentifier(this.m_underlyingConnection.ConnectionSocket, addAIMessage);
            this.m_underlyingConnection.SetIdentifierReceiver(addAIMessage.Identifier, msg => { /*Trace.WriteLine(msg);*/ responseQuery = msg.Argument1; });
            this.m_underlyingConnection.SendMessage(addAIMessage);
            while (responseQuery is null && (timeout >= 0 || timeout == -1)) {
                await Task.Delay(1);
                if (timeout != -1) {
                    timeout--;
                }
            }
            if (int.TryParse(responseQuery, out int id)) {
                return id;
            } else {
                return -1;
            }
        }

        /// <summary>
        /// Try and create a new <see cref="AILobbyMember"/> in the <see cref="ManagedLobby"/>. This will not create the <see cref="AILobbyMember"/> until the <see cref="MessageType.LOBBY_AIJOIN"/> message is received.
        /// </summary>
        /// <param name="difficulty">The difficulty of the AI.</param>
        /// <param name="faction">The faction of the AI.</param>
        /// <param name="teamIndex">The index (position) of the AI on the given team.</param>
        /// <returns>The ID given to the <see cref="AILobbyMember"/> by the server.</returns>
        /// <exception cref="PermissionDeniedException"/>
        public int CreateAIPlayer(AIDifficulty difficulty, string faction, ManagedLobbyTeamType teamIndex) 
            => Task.Run(() => this.TryCreateAIPlayer(difficulty, faction, (int)teamIndex, 1250)).Result;

        /// <summary>
        /// Kick a player from the lobby. (If host).
        /// </summary>
        /// <param name="playerID">The unique steam ID of the player to kick.</param>
        /// <param name="message">A message to send to the user stating why they were kicked.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void KickPlayer(ulong playerID, string message = "Kicked by Host")
            => this.m_isHost.Then(() => this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_KICK, playerID.ToString(), message)))
            .Else(() => throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY));

        /// <summary>
        /// Remove the AI player with specified ID. (If host).
        /// </summary>
        /// <param name="aiID">The ID used to identify the AI to remove.</param>
        /// <exception cref="PermissionDeniedException"/>
        public void RemoveAI(ulong aiID)
            => this.m_isHost.Then(() => this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_REMOVEAI, aiID.ToString())))
            .Else(() => throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY));

        /// <summary>
        /// Remove a player from the <see cref="ManagedLobby"/>.
        /// </summary>
        /// <param name="id">The unique ID of the player to remove.</param>
        /// <param name="broadcast">Should try to broadcast this event. (Host only action).</param>
        /// <exception cref="PermissionDeniedException"/>
        public void RemovePlayer(ulong id, bool broadcast) {
            var player = this.TryFindPlayerFromID(id, out ManagedLobbyTeamType team);
            if (player is HumanLobbyMember human) {
                broadcast.Then(() => this.m_isHost.Then(() => this.KickPlayer(id)).Else(() => throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY)));
                this.m_teams[team].Leave(human);
            } else if (player is AILobbyMember ai) {
                broadcast.Then(() => this.m_isHost.Then(() => this.RemoveAI(ai.ID)).Else(() => throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY)));
                this.m_teams[team].Leave(ai);
            } else {
                Trace.WriteLine($"Unable to find player with ID = {id}", "ManagedLobby");
            }
        }

        private void OnPlayerJoined(ulong steamID, string name) {

            HumanLobbyMember newPlayer = new HumanLobbyMember(this, steamID, name, string.Empty, 0.0);

            bool alliesHasLessThanAxis = this.m_teams[ManagedLobbyTeamType.Allies].Count <= this.m_teams[ManagedLobbyTeamType.Axis].Count;
            ManagedLobbyTeamType teamToJoin = alliesHasLessThanAxis ? ManagedLobbyTeamType.Allies : ManagedLobbyTeamType.Axis;

            if (this.m_teams[teamToJoin].Join(newPlayer)) {
                Trace.WriteLine($"{name} has joined team {teamToJoin}");
                if (teamToJoin == ManagedLobbyTeamType.Axis) { // if axis team was joined, update faction
                    this.SetFaction(steamID, Faction.Wehrmacht.Name);
                }
                if (this.m_isHost) {
                    this.SendMetaMessage($"{steamID}-join-success");
                }
            } else {
                this.m_teams[ManagedLobbyTeamType.Spectator].Join(newPlayer, !this.m_isHost);
            }

        }

        /// <summary>
        /// Compile the win condition using data from the lobby members and begin the match with all lobby members.<br/>This will start Company of Heroes 2 if completed.
        /// </summary>
        /// <remarks>The method is synchronous and make take several minutes to complete. (Use in a <see cref="Task.Run(Action)"/> context to maintain responsiveness).</remarks>
        /// <param name="operationCancelled">The <see cref="Action{T}"/> invoked if the execution of the method is cancelled. The <see cref="string"/> argument describes what caused the cancellation.</param>
        public async void CompileAndStartMatch(Action<string> operationFeedback) {

            // Make sure we're the host
            if (!this.m_isHost) {
                throw new PermissionDeniedException(PermissionDeniedException.HOST_ONLY);
            }

            // Get the local company
            Company ownCompany = GetLocalCompany();
            if (ownCompany == null) {
                operationFeedback?.Invoke("Failed to load own company!");
                return;
            } else {
                ownCompany.Owner = BattlegroundsInstance.LocalSteamuser.ID.ToString();
            }

            // Send a "Starting match" message to lobby members
            this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_STARTING));

            // Wait a bit
            await Task.Delay(240);

            // Log
            operationFeedback?.Invoke("Downloading lobby companies");

            // Get company lobbies
            (List<Company> lobbyCompanies, bool success) = await GetLobbyCompanies();

            // If we managed to retrieve companies
            if (success) {

                // Add our own
                lobbyCompanies.Add(ownCompany);

                // The session to be built
                Session session = null;

                try {

                    // Get match data (A SessionInfo object)
                    if (this.OnLocalDataRequested?.Invoke("MatchInfo") is SessionInfo info) {

                        // Zip/Map the companies to their respective SessionParticipant instances.
                        Session.ZipCompanies(lobbyCompanies.ToArray(), ref info);

                        // Create the session properly.
                        session = Session.CreateSession(info);

                    } else {
                        throw new Exception("Failed to get a valid SessionInfo");
                    }

                } catch (Exception e) {
                    operationFeedback?.Invoke(e.Message);
                }

                // Did we fail to create session?
                if (session is null) {
                    operationFeedback?.Invoke("Failed to create session");
                }

                // Play the session
                SessionManager.PlaySession<SessionCompiler<CompanyCompiler>, CompanyCompiler>(
                    session,
                    (a, b) => { operationFeedback?.Invoke(a.ToString()); this.ManagedLobbyInternal_GameSessionStatusChanged(a, b); },
                    this.ManagedLobbyInternal_GameMatchAnalyzed,
                    () => this.ManagedLobbyInternal_GameOnGamemodeCompiled(operationFeedback));

            } else {
                operationFeedback?.Invoke("Failed to get lobby companies");
            }

        }

        private bool ManagedLobbyInternal_GameOnGamemodeCompiled(Action<string> operationCancelled) {

            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

            if (File.Exists(sgapath)) {

                // Upload
                if (FileHub.UploadFile(sgapath, "gamemode.sga", this.LobbyFileID)) {

                    // Sleep for 1s (TODO: Fix Upload to wait for confirmation...)
                    Thread.Sleep(1000);

                    // Notify lobby players the gamemode is available
                    this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_NOTIFY_GAMEMODE));

                    // Sleep for 1s
                    Thread.Sleep(1000);

                    // Send the start match...
                    this.m_underlyingConnection.SendMessage(new Message(MessageType.LOBBY_STARTMATCH));

                    // Sleep for 1s
                    Thread.Sleep(1000);

                    // Return true
                    return true;

                } else {

                    operationCancelled?.Invoke("Failed to upload gamemode!");

                }

            } else {
                operationCancelled?.Invoke("Failed to compile!");
            }

            return false;

        }

        private void ManagedLobbyInternal_GameSessionStatusChanged(SessionStatus status, Session s) {
            Trace.WriteLine($"Session update: {status}", "ManagedLobby");
            switch (status) {
                case SessionStatus.S_FailedCompile:
                    break;
                case SessionStatus.S_FailedPlay:
                    break;
                case SessionStatus.S_GameNotLaunched:
                    break;
                case SessionStatus.S_NoPlayback:
                    break;
                case SessionStatus.S_ScarError:
                    break;
                case SessionStatus.S_BugSplat:
                    break;
            }
        }

        private void ManagedLobbyInternal_GameMatchAnalyzed(GameMatch results) {
            // TODO: Sync
        }

        private void ManagedLobbyInternal_MessageReceived(Message incomingMessage) {
            switch (incomingMessage.Descriptor) {
                case MessageType.LOBBY_CHATMESSAGE:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Message, incomingMessage.Argument2, incomingMessage.Argument1);
                    break;
                case MessageType.LOBBY_METAMESSAGE:
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Meta, incomingMessage.Argument2, incomingMessage.Argument1);
                    break;
                case MessageType.LOBBY_JOIN:
                    if (ulong.TryParse(incomingMessage.Argument2, out ulong newSteamID)) {
                        this.OnPlayerJoined(newSteamID, incomingMessage.Argument1);
                        this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Join, incomingMessage.Argument1, incomingMessage.Argument2);
                    } else {
                        Trace.WriteLine($"Failed to connect player with invalid steam ID {incomingMessage.Argument2}", "ManagedLobby.cs");
                    }
                    break;
                case MessageType.LOBBY_LEAVE:
                    if (ulong.TryParse(incomingMessage.Argument2, out ulong leavingPlayerID)) {
                        this.RemovePlayer(leavingPlayerID, false);
                    }
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Leave, incomingMessage.Argument1, string.Empty);
                    break;
                case MessageType.LOBBY_KICK:
                    if (ulong.TryParse(incomingMessage.Argument2, out ulong kickedPlayerID)) {
                        this.RemovePlayer(kickedPlayerID, false);
                    }
                    this.OnPlayerEvent?.Invoke(ManagedLobbyPlayerEventType.Kicked, incomingMessage.Argument1, incomingMessage.Argument2);
                    break;
                case MessageType.LOBBY_KICKED:
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.Kicked, incomingMessage.Argument1);
                    break;
                case MessageType.LOBBY_SETHOST:
                    this.m_isHost = true;
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.Host, string.Empty);
                    break;
                case MessageType.LOBBY_UNSETHOST:
                    this.m_isHost = false;
                    this.OnLocalEvent?.Invoke(ManagedLobbyLocalEventType.HostRemove, string.Empty);
                    break;
                case MessageType.LOBBY_INFO:
                    this.LobbyInfoChange(incomingMessage.Argument1, incomingMessage.Argument2);
                    break;
                case MessageType.LOBBY_REQUEST_COMPANY:
                    this.OnDataRequest?.Invoke(true, incomingMessage.Argument1, "CompanyData", incomingMessage.Identifier);
                    break;
                case MessageType.LOBBY_REQUEST_RESULTS:
                    this.OnDataRequest?.Invoke(true, incomingMessage.Argument1, "MatchData", incomingMessage.Identifier);
                    break;
                case MessageType.LOBBY_STARTMATCH:
                    this.OnStartMatchReceived?.Invoke();
                    break;
                case MessageType.CONFIRMATION_MESSAGE:
                    break;
                case MessageType.LOBBY_NOTIFY_GAMEMODE:
                    string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";
                    if (!FileHub.DownloadFile(path, "gamemode.sga", this.LobbyFileID)) {
                        Trace.WriteLine("Failed to download 'gamemode.sga'", "ManagedLobby");
                    } else {
                        Trace.WriteLine("Successfully downloaded 'gamemode.sga'", "ManagedLobby");
                    }                    
                    break;
                case MessageType.LOBBY_AIJOIN:
                    ManagedLobbyTeamType aiteam = ManagedLobbyTeam.GetTeamTypeFromFaction(incomingMessage.Argument2);
                    this.m_teams[aiteam].Join(new AILobbyMember(this, Enum.Parse<AIDifficulty>(incomingMessage.Argument1), incomingMessage.Argument2, ulong.Parse(incomingMessage.Argument3)));
                    break;
                case MessageType.LOBBY_AILEAVE:
                    if (ulong.TryParse(incomingMessage.Argument2, out ulong aiid)) {
                        var aiplayer = this.TryFindPlayerFromID(aiid);
                        if (aiplayer is AILobbyMember) {
                            var team = ManagedLobbyTeam.GetTeamTypeFromFaction(aiplayer.Faction);
                            this.m_teams[team].Leave(aiplayer);
                        } else {
                            Trace.WriteLine($"Failed to remove AI with ID {aiid} (May already have been removed on the clientside).", "ManagedLobby");
                        }
                    } else {
                        Trace.WriteLine("Failed to remove AI (Invalid ID)", "ManagedLobby");
                    }
                    break;
                case MessageType.LOBBY_STARTING:
                    this.OnMatchInfoReceived?.Invoke(incomingMessage.Descriptor.ToString(), incomingMessage.Argument1, incomingMessage.Argument2);
                    break;
                case MessageType.LOBBY_CHANGEDUSERDATA:
                    this.LobbyUserInfoChanged(incomingMessage.Argument3, incomingMessage.Argument1, incomingMessage.Argument2);
                    break;
                case MessageType.FatalMessageError:
                    break;
                default: Trace.WriteLine($"Unhandled type <<{incomingMessage.Descriptor}>>", "ManagedLobby"); break;
            }
        }

        private void LobbyInfoChange(string info, string value) {
            switch (info) {
                case "selected_map":
                    this.m_lobbyMap = value;
                    break;
                case "selected_wc":
                    this.m_lobbyGamemode = value;
                    break;
                case "selected_wcs":
                    this.m_lobbyGamemodeOption = value;
                    break;
                case "capacity":
                    break;
                default:
                    Trace.WriteLine($"Unknown lobby information change: {info} (with value '{value}')", "ManagedLobby");
                    break;
            }
            this.OnLobbyInfoChanged?.Invoke(info, value);
        }

        private void LobbyUserInfoChanged(string user, string info, string value) {
            if (ulong.TryParse(user, out ulong id)) {
                if (this.TryFindPlayerFromID(id) is ManagedLobbyMember member) {
                    switch (info) {
                        case "fac":
                            member.UpdateFaction(value);
                            break;
                        case "str":
                            member.UpdateCompany(member.CompanyName, double.Parse(value));
                            break;
                        case "com":
                            member.UpdateCompany(value, member.CompanyStrength);
                            break;
                        case "pos":
                            break;
                        default:
                            Trace.WriteLine(info + ":" + value);
                            break;
                    }
                } else {
                    Trace.WriteLine($"Failed to parse user ID \"{user}#{id}\"", "ManagedLobby");
                }
            } else {
                Trace.WriteLine($"Failed to parse user ID \"{user}\"", "ManagedLobby");
            }
        }

        private ManagedLobbyMember TryFindPlayerFromID(ulong id, out ManagedLobbyTeamType team) {
            if (this.m_teams[ManagedLobbyTeamType.Allies].GetLobbyMember(id) is ManagedLobbyMember allies) {
                team = ManagedLobbyTeamType.Allies;
                return allies;
            } else if (this.m_teams[ManagedLobbyTeamType.Axis].GetLobbyMember(id) is ManagedLobbyMember axis) {
                team = ManagedLobbyTeamType.Axis;
                return axis;
            } else {
                team = ManagedLobbyTeamType.Spectator;
                return this.m_teams[ManagedLobbyTeamType.Spectator].GetLobbyMember(id);
            }
        }

        /// <summary>
        /// Try and find the <see cref="ManagedLobbyMember"/> tied to the unique ID.
        /// </summary>
        /// <param name="id">The unique ID to find member from.</param>
        /// <returns>The <see cref="ManagedLobbyMember"/> with given unique ID. Otherwise <see langword="null"/>.</returns>
        public ManagedLobbyMember TryFindPlayerFromID(ulong id) => this.TryFindPlayerFromID(id, out _);

        /// <summary>
        /// Invoke a method on this instance after a specified delay.
        /// </summary>
        /// <param name="delay">The delay in milliseconds</param>
        /// <param name="action">The action to call</param>
        public async void InvokeDelayed(int delay, Action<ManagedLobby> action) {
            await Task.Delay(delay);
            action.Invoke(this);
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
            if (lobbyPassword is null) {
                lobbyPassword = string.Empty;
            }

            // Callback for establishing connection
            void OnConnectionEstablished(bool connected, Connection connection) {
                if (connected) {
                    void OnLobbyCreated(Socket _, Message response) {
                        if (response.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                            var mLobby = new ManagedLobby(connection, true) {
                                m_lobbyID = response.Argument2,
                                m_self = hub.User,
                            };
                            mLobby.SetLobbyCapacity(2, false);
                            mLobby.m_teams[ManagedLobbyTeamType.Allies].Join(new HumanLobbyMember(mLobby, hub.User.ID, hub.User.Name, string.Empty, 0.0));
                            managedCallback?.Invoke(new ManagedLobbyStatus(true), mLobby);
                        } else {
                            managedCallback?.Invoke(new ManagedLobbyStatus(false, response.Argument1), null);
                        }
                    }
                    MessageSender.SendMessage(connection.ConnectionSocket, new Message(MessageType.LOBBY_CREATE, lobbyName, lobbyPassword), OnLobbyCreated);
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
                throw new ArgumentNullException(nameof(hub), "The lobby hub instance was null!");
            }

            // Make sure the GUID is valid.
            if (lobbyGUID.Length != 36) {
                throw new ArgumentOutOfRangeException(nameof(lobbyGUID), "The GUID was not of length 36 and is therefore not a valid GUID.");
            }

            // Make sure the password is null
            if (password is null) {
                throw new ArgumentNullException(nameof(password), "Password cannot be null. Use String.Empty if no password is used");
            }

            // Callback for establishing connection
            void OnConnectionEstablished(bool connected, Connection connection) {
                if (connected) {
                    void OnLobbyJoinResponse(Socket _, Message response) {
                        if (response.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                            managedCallback?.Invoke(new ManagedLobbyStatus(true), new ManagedLobby(connection, false) { 
                                m_lobbyID = response.Argument2,
                                m_self = hub.User,
                            });
                        } else {
                            managedCallback?.Invoke(new ManagedLobbyStatus(false, response.Argument1), null);
                        }
                    }
                    MessageSender.SendMessage(connection.ConnectionSocket, new Message(MessageType.LOBBY_JOIN, lobbyGUID, password), OnLobbyJoinResponse);
                } else {
                    managedCallback?.Invoke(new ManagedLobbyStatus(false, "Unable to establish connection with server."), null);
                }
            }

            // Connect
            hub.Connect(OnConnectionEstablished);

        }

    }

}
