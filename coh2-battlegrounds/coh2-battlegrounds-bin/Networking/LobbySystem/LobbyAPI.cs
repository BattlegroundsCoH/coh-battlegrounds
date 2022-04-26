using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

using static Battlegrounds.Networking.LobbySystem.LobbyAPIStructs;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Event handler for <see cref="LobbyAPI"/> events.
/// </summary>
public delegate void LobbyEventHandler();

/// <summary>
/// Event handler for <see cref="LobbyAPI"/> events with an argument object.
/// </summary>
/// <typeparam name="T">The argument object type.</typeparam>
/// <param name="args">The arguments to pass aling to the handler.</param>
public delegate void LobbyEventHandler<T>(T args);

/// <summary>
/// Event arguments to a startup cancelled event.
/// </summary>
/// <param name="CancelId">The ID of the user who cancelled the startup.</param>
/// <param name="CancelName">The name of the user who cancelled the startup.</param>
public record LobbyMatchStartupCancelledEventArgs(ulong CancelId, string CancelName);

/// <summary>
/// Event arguments to the settings changed event.
/// </summary>
/// <param name="SettingsKey">The setting that was changed.</param>
/// <param name="SettingsValue">The new value of the setting.</param>
public record LobbySettingsChangedEventArgs(string SettingsKey, string SettingsValue);

/// <summary>
/// Event arguments to the system message event.
/// </summary>
/// <param name="MemberId">The ID of the user who caused the message.</param>
/// <param name="SystemMessage">The message to display.</param>
/// <param name="SystemContext">The context of the system message.</param>
public record LobbySystemMessageEventArgs(ulong MemberId, string SystemMessage, string SystemContext);

/// <summary>
/// Event arguments for the company changed event.
/// </summary>
/// <param name="TeamId">The team of the changed company.</param>
/// <param name="SlotId">The slot of the changed company.</param>
/// <param name="Company">The new company.</param>
public record LobbyCompanyChangedEventArgs(int TeamId, int SlotId, LobbyCompany Company);

/// <summary>
/// Event arguments for match the event a match is halted.
/// </summary>
/// <param name="IsError">Flag marking whether the infor is an error or general information.</param>
/// <param name="Type">The type of halt (Where in the match process we were halted, and the severity)</param>
/// <param name="Reason">The reason given for the halt.</param>
public record LobbyMatchInfoEventArgs(bool IsError, string Type, string Reason);

/// <summary>
/// Class for interacting with the lobby logic on the server.
/// </summary>
public sealed class LobbyAPI {

    public const string SETTING_MAP = "selected_map";
    public const string SETTING_GAMEMODE = "selected_wc";
    public const string SETTING_GAMEMODEOPTION = "selected_wco";
    public const string SETTING_WEATHER = "selected_daynight";
    public const string SETTING_LOGISTICS = "selected_supply";
    public const string SETTING_MODPACK = "selected_tuning";

    private static readonly TimeZoneInfo __thisTimezone = TimeZoneInfo.Local;

    private readonly ServerConnection m_connection;
    private readonly bool m_isHost;
    private volatile uint m_cidcntr;

    private LobbyTeam m_allies;
    private LobbyTeam m_axis;
    private LobbyTeam m_obs;
    private readonly Dictionary<string, string> m_settings;

    /// <summary>
    /// Get the title of the joined lobby.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Get if the API should act with host privilege.
    /// </summary>
    public bool IsHost => this.m_isHost;

    /// <summary>
    /// Get the allied team.
    /// </summary>
    public LobbyTeam Allies => this.m_allies;

    /// <summary>
    /// Get the axis team.
    /// </summary>
    public LobbyTeam Axis => this.m_axis;

    /// <summary>
    /// Get the observer team.
    /// </summary>
    public LobbyTeam Observers => this.m_obs;

    /// <summary>
    /// Get (or set) the settings of the lobby.
    /// </summary>
    public Dictionary<string, string> Settings => this.m_settings;

    /// <summary>
    /// Get the handle to the <see cref="ServerAPI"/> instance.
    /// </summary>
    public ServerAPI ServerHandle { get; }

    /// <summary>
    /// Get the self-user instance.
    /// </summary>
    public SteamUser Self { get; }

    /// <summary>
    /// Event triggered when a lobby chat message is received.
    /// </summary>
    public event LobbyEventHandler<LobbyMessage>? OnChatMessage;

    /// <summary>
    /// Event triggered when the system announces a message.
    /// </summary>
    public event LobbyEventHandler<LobbySystemMessageEventArgs>? OnSystemMessage;

    /// <summary>
    /// Event triggered when a lobby team instance is changed.
    /// </summary>
    public event LobbyEventHandler<LobbyTeam>? OnLobbyTeamUpdate;

    /// <summary>
    /// Event triggered when a lobby slot isntance is changed.
    /// </summary>
    public event LobbyEventHandler<LobbySlot>? OnLobbySlotUpdate;

    /// <summary>
    /// Event triggered when a lobby company is changed.
    /// </summary>
    public event LobbyEventHandler<LobbyCompanyChangedEventArgs>? OnLobbyCompanyUpdate;

    /// <summary>
    /// Event triggered when the lobby settings have been changed.
    /// </summary>
    public event LobbyEventHandler<LobbySettingsChangedEventArgs>? OnLobbySettingUpdate;

    /// <summary>
    /// Event triggered when the connection to the lobby was lost.
    /// </summary>
    public event LobbyEventHandler<string>? OnLobbyConnectionLost;

    /// <summary>
    /// Event triggered when the match startup has been cancelled.
    /// </summary>
    public event LobbyEventHandler<LobbyMatchStartupCancelledEventArgs>? OnLobbyCancelStartup;

    /// <summary>
    /// Event triggered when the host has pressed the "begin match" button.
    /// </summary>
    public event LobbyEventHandler? OnLobbyBeginMatch;

    /// <summary>
    /// Event triggered when the game should be launched.
    /// </summary>
    public event LobbyEventHandler? OnLobbyLaunchGame;

    /// <summary>
    /// Event triggered when the lobby has sent a request for the local machine's company file.
    /// </summary>
    public event LobbyEventHandler<ServerAPI>? OnLobbyRequestCompany;

    /// <summary>
    /// Event triggered when the lobby has been asked to notify the local machine the gamemode is ready for download.
    /// </summary>
    public event LobbyEventHandler<ServerAPI>? OnLobbyNotifyGamemode;

    /// <summary>
    /// Event triggered when the lobby has been asked to notify the local machine the results of the latest match are available.
    /// </summary>
    public event LobbyEventHandler<ServerAPI>? OnLobbyNotifyResults;

    /// <summary>
    /// Event triggered when the server has sent a countdown event.
    /// </summary>
    public event LobbyEventHandler<int>? OnLobbyCountdown;

    /// <summary>
    /// Event triggered when the host has sent error information regarding the match.
    /// </summary>
    public event LobbyEventHandler<LobbyMatchInfoEventArgs>? OnLobbyMatchError;

    /// <summary>
    /// Event triggered when the host has sent information regarding the match.
    /// </summary>
    public event LobbyEventHandler<LobbyMatchInfoEventArgs>? OnLobbyMatchInfo;

    /// <summary>
    /// Event triggerd when the participants report their ready status.
    /// </summary>
    public event LobbyEventHandler<string>? OnPoll;

    /// <summary>
    /// Event triggered when the host has sent a lobby halt message
    /// </summary>
    public event LobbyEventHandler? OnLobbyMatchHalt;

    /// <summary>
    /// Initialises a new <see cref="LobbyAPI"/> instance that is connected along a <see cref="ServerConnection"/> to a lobby.
    /// </summary>
    /// <param name="isHost">Flag marking if host. This is for local checking, the server verifies this independently.</param>
    /// <param name="title">The title of the lobby that was joined or hosted.</param>
    /// <param name="self">The <see cref="SteamUser"/> instance that represents the local machine.</param>
    /// <param name="connection">The connection that connects the local machine to the server.</param>
    /// <param name="serverAPI">The API object that performs HTTP API calls to the server.</param>
    /// <exception cref="Exception"></exception>
    public LobbyAPI(bool isHost, string title, SteamUser self, ServerConnection connection, ServerAPI serverAPI) {

        // Store ref to server handle
        this.ServerHandle = serverAPI;

        // Set internal refs
        this.m_connection = connection;
        this.m_isHost = isHost;
        this.m_cidcntr = 1000;

        // Store self (id)
        this.Self = self;

        // Store title
        this.Title = title;

        // Add private hook
        this.m_connection.MessageReceived = this.OnMessage;

        // Create get lobby message
        Message msg = new Message() {
            CID = this.m_cidcntr++,
            Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = "GetLobby", Arguments = Array.Empty<string>() }),
            Mode = MessageMode.BrokerCall,
            Sender = this.m_connection.SelfID,
            Target = 0
        };

        // #1 reason why we should create the API instance on a separate thread
        for (int i = 0; i < 5; i++) {

            // Make pull lobby request
            ContentMessage? reply = this.m_connection.SendAndAwaitReply(msg);
            if (reply is ContentMessage response && GoMarshal.JsonUnmarshal<LobbyRemote>(response.Raw) is LobbyRemote remoteLobby) {

                // Make sure teams are valid
                if (remoteLobby.Teams is null || remoteLobby.Teams.Any(x => x is null)) {
                    throw new Exception("Received **NULL** team data.");
                }

                // Set API on team objects
                remoteLobby.Teams.ForEach(x => x.SetAPI(this));

                // Create team data
                this.m_allies = remoteLobby.Teams[0];
                this.m_axis = remoteLobby.Teams[1];
                this.m_obs = remoteLobby.Teams[2];
                this.m_settings = remoteLobby.Settings;

                // Register connection lost
                this.m_connection.OnConnectionLost += _ => {
                    this.OnLobbyConnectionLost?.Invoke("LOST");
                };

                // Return all OK
                return;

            }

            // Sleep
            Thread.Sleep(50);

            // Log
            Trace.WriteLine("Resending request to get light-lobby version...", nameof(LobbyAPI));

        }

        // Disconnect
        this.Disconnect();

        // Throw exception -> Failed to fully connect.
        throw new Exception("Failed to retrieve light-lobby version.");

    }

    private LobbyTeam? GetTeamInstanceByIndex(int tid) => tid switch {
        0 => this.m_allies,
        1 => this.m_axis,
        2 => this.m_obs,
        _ => throw new IndexOutOfRangeException($"Team ID '{tid}' is out of bounds.")
    };

    private void OnMessage(uint cid, ulong sender, ContentMessage message) {

        // Check if error message and display it
        if (message.MessageType is ContentMessgeType.Error) {
            Trace.WriteLine($"Received Error = {message.StrMsg} to CID({cid})", nameof(LobbyAPI));
            return;
        }

        // If disconnect, handle
        if (message.MessageType is ContentMessgeType.Disconnect) {
            if (message.Who == this.m_connection.SelfID) {
                this.OnLobbyConnectionLost?.Invoke(message.Kick ? "KICK" : "CLOSED");
            } else {
                this.OnSystemMessage?.Invoke(new(message.Who, message.StrMsg, message.Kick ? "KICK" : "LEFT"));
            }
            return;
        }

        // If join, handle
        if (message.MessageType is ContentMessgeType.Join) {
            this.OnSystemMessage?.Invoke(new(message.Who, message.StrMsg, "JOIN"));
            return;
        }

        // Switch on strmsg
        switch (message.StrMsg) {
            case "Message":

                if (GoMarshal.JsonUnmarshal<LobbyMessage>(message.Raw) is LobbyMessage lobbyMessage) {

                    // Get serverzone
                    var serverZone = TimeZoneInfo.FindSystemTimeZoneById(lobbyMessage.Timezone);
                    bool isDaylightSaving = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Parse(lobbyMessage.Timestamp));

                    // Create timestamp
                    var datetime = FromTimestamp(lobbyMessage.Timestamp) + (__thisTimezone.BaseUtcOffset - serverZone.BaseUtcOffset);

                    if (isDaylightSaving) {
                        datetime = datetime.AddHours(1);
                    }

                    lobbyMessage.Timestamp = $"{datetime.Hour:00}:{datetime.Minute:00}";

                    // Trigger event
                    this.OnChatMessage?.Invoke(lobbyMessage);

                }

                break;
            case "Notify.Company":

                // Get call
                var companyCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                // Get args
                bool parsed = int.TryParse(companyCall.Arguments[0], out int tid);
                parsed &= int.TryParse(companyCall.Arguments[1], out int sid);
                if (!parsed) {
                    Trace.WriteLine($"Failed to parse '{companyCall.Arguments[0]}' or '{companyCall.Arguments[1]}' as integers", nameof(LobbyAPI));
                    return;
                }

                // Get strength
                if (!float.TryParse(companyCall.Arguments[6], out float strength)) {
                    Trace.WriteLine($"Failed to parse '{companyCall.Arguments[6]}' as float32", nameof(LobbyAPI));
                    return;
                }

                // Create company
                var company = new LobbyCompany() {
                    API = this,
                    IsAuto = companyCall.Arguments[2] == "1",
                    IsNone = companyCall.Arguments[3] == "1",
                    Name = companyCall.Arguments[4],
                    Army = companyCall.Arguments[5],
                    Strength = strength,
                    Specialisation = companyCall.Arguments[7]
                };

                // Set company API
                company.SetAPI(this);

                // Invoke handler
                this.OnLobbyCompanyUpdate?.Invoke(new(tid, sid, company));

                break;
            case "Notify.Team":

                // Try get team
                if (GoMarshal.JsonUnmarshal<LobbyTeam>(message.Raw) is LobbyTeam newTeam) {

                    // Set API
                    newTeam.SetAPI(this);

                    // Refresh self teams
                    if (newTeam.TeamID == 0) {
                        this.m_allies = newTeam;
                    } else if (newTeam.TeamID == 1) {
                        this.m_axis = newTeam;
                    } else {
                        this.m_obs = newTeam;
                    }

                    // Trigger team update
                    this.OnLobbyTeamUpdate?.Invoke(newTeam);

                }

                break;
            case "Notify.Slot":

                // Try get slot
                if (GoMarshal.JsonUnmarshal<LobbySlot>(message.Raw) is LobbySlot newSlot) {

                    // Set API
                    newSlot.SetAPI(this);

                    // Update slot on team
                    switch (newSlot.TeamID) {
                        case 0:
                            this.m_allies.Slots[newSlot.SlotID] = newSlot;
                            break;
                        case 1:
                            this.m_axis.Slots[newSlot.SlotID] = newSlot;
                            break;
                    }

                    // Trigger slot update
                    this.OnLobbySlotUpdate?.Invoke(newSlot);

                }

                break;
            case "Notify.Setting":

                // Get call
                var settingCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                // Decode
                var (settingKey, settingValue) = settingCall.Decode<string, string>();

                // Notify
                this.OnLobbySettingUpdate?.Invoke(new(settingKey, settingValue));

                break;
            case "Notify.Start":
                this.OnLobbyBeginMatch?.Invoke();
                break;
            case "Notify.Cancel":
                this.OnLobbyCancelStartup?.Invoke(new(sender, Encoding.UTF8.GetString(message.Raw)));
                break;
            case "Notify.Launch":
                this.OnLobbyLaunchGame?.Invoke();
                break;
            case "Request.Company":
                this.OnLobbyRequestCompany?.Invoke(this.ServerHandle);
                break;
            case "Notify.Gamemode":
                this.OnLobbyNotifyGamemode?.Invoke(this.ServerHandle);
                break;
            case "Notify.Results":
                this.OnLobbyNotifyResults?.Invoke(this.ServerHandle);
                break;
            case "Notify.Countdown":
                this.OnLobbyCountdown?.Invoke(message.RemoteAction);
                break;
            case "Notify.HaltMatch":
                this.OnLobbyMatchHalt?.Invoke();
                break;
            case "Notify.ErrorMatch":

                // Get call
                var errMatchCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                // Invoke event
                this.OnLobbyMatchError?.Invoke(new(true, errMatchCall.Arguments[0], errMatchCall.Arguments[1]));

                break;
            case "Notify.InfoMatch":

                // Get call
                var infoMatchCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                // Invoke event
                this.OnLobbyMatchInfo?.Invoke(new(false, infoMatchCall.Arguments[0], infoMatchCall.Arguments[1]));

                break;
            case "Notify.Poll":

                // Get call
                var pollCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                // Invoke event
                this.OnPoll?.Invoke(new(pollCall.Arguments[0]));

                break;
            default:
                Trace.WriteLine($"Unsupported API event: {message.StrMsg}", nameof(LobbyAPI));
                break;
        }

    }

    private static DateTime FromTimestamp(string stamp) {
        string[] s = stamp.Split(':');
        int h = int.Parse(s[0]);
        int m = int.Parse(s[1]);
        return DateTime.Today.AddHours(h).AddMinutes(m);
    }

    /// <summary>
    /// Disconnects the local machine from the lobby.
    /// </summary>
    public void Disconnect() {
        this.m_connection.Shutdown();
    }

    /// <summary>
    /// Gets the amount of players in the lobby.
    /// </summary>
    /// <param name="humansOnly">Only cound human players.</param>
    /// <returns>The mount of players, and if the <paramref name="humansOnly"/> flag is not set, also the amount of AI players.</returns>
    public uint GetPlayerCount(bool humansOnly = false)
        => this.RemoteCall<uint>("GetPlayerCount", EncBool(humansOnly));

    private static string EncBool(bool b) => b ? "1" : "0";

    /// <summary>
    /// Set the company of the specified slot.
    /// </summary>
    /// <param name="tid">The team ID to set company for. In the range 0 &#x2264; T &#x2264; 1</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4</param>
    /// <param name="company">The company to set.</param>
    public void SetCompany(int tid, int sid, LobbyCompany company) {

        // Convert to str
        string strength = company.Strength.ToString(CultureInfo.InvariantCulture);
        string auto = EncBool(company.IsAuto);
        string none = EncBool(company.IsNone);

        // Invoke remotely
        this.RemoteVoidCall("SetCompany", tid, sid, auto, none, company.Name, company.Army, strength, company.Specialisation);

        // Trigger self update
        this.OnLobbyCompanyUpdate?.Invoke(new(tid, sid, company)); // This might need to be removed!

    }

    /// <summary>
    /// Moves a member to specified team and slot.
    /// </summary>
    /// <param name="mid">The ID of the member that is moving.</param>
    /// <param name="tid">The team ID containing the slot being move to. Accepts values in the range 0 &#x2264; T &#x2264; 2</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4 that the member should move to.</param>
    public void MoveSlot(ulong mid, int tid, int sid)
        => this.RemoteVoidCall("MoveSlot", mid, tid, sid);

    /// <summary>
    /// Updates the state of a lobby member.
    /// </summary>
    /// <param name="mid">The ID of the member that is moving.</param>
    /// <param name="tid">The team ID containing the slot being move to. Accepts values in the range 0 &#x2264; T &#x2264; 2</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4 that the member should move to.</param>
    /// <param name="state">The new state of the member.</param>
    public void MemberState(ulong mid, int tid, int sid, LobbyMemberState state)
        => this.RemoteVoidCall("MemberState", mid, tid, sid, (byte)state);

    /// <summary>
    /// Add an AI player to the specified slow.
    /// </summary>
    /// <param name="tid">The ID of the team to add AI to. Accepts values in the range 0 &#x2264; T &#x2264; 1</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4</param>
    /// <param name="difficulty">The AI difficulty level (integer representation of <see cref="AIDifficulty"/>)</param>
    /// <param name="company">The company initially given to the AI.</param>
    /// <exception cref="InvokePermissionAccessDeniedException"></exception>
    public void AddAI(int tid, int sid, int difficulty, LobbyCompany company) {
    
        // Make sure we can
        if (!this.m_isHost) {
            throw new InvokePermissionAccessDeniedException("Cannot invoke remote method that requires host-privellige");
        }

        // Get invariant strength
        var inv = company.Strength.ToString(CultureInfo.InvariantCulture);

        // Call AI
        RemoteVoidCall("AddAI", tid, sid, difficulty, EncBool(company.IsAuto), EncBool(company.IsNone), company.Name, company.Army, inv, company.Specialisation);

    }

    /// <summary>
    /// Removes an occupant from the lobby.
    /// </summary>
    /// <param name="tid">The team ID containing the occupant to be removed. Accepts values in the range 0 &#x2264; T &#x2264; 2</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4</param>
    /// <exception cref="InvokePermissionAccessDeniedException"></exception>
    public void RemoveOccupant(int tid, int sid) {

        // Make sure we can
        if (!this.m_isHost) {
            throw new InvokePermissionAccessDeniedException("Cannot invoke remote method that requires host-privellige");
        }

        // Trigger remote call
        this.RemoteVoidCall("RemoveOccupant", tid, sid);

    }

    /// <summary>
    /// Locks the unlocked slot at specified position.
    /// </summary>
    /// <param name="tid">The team ID containing the slot to be locked. Accepts values in the range 0 &#x2264; T &#x2264; 1</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4</param>
    public void LockSlot(int tid, int sid)
        => this.RemoteVoidCall("LockSlot", tid, sid);

    /// <summary>
    /// Unlocks the locked slot at specified position.
    /// </summary>
    /// <param name="tid">The team ID containing the slot to be unlocked. Accepts values in the range 0 &#x2264; T &#x2264; 1</param>
    /// <param name="sid">The slot ID in the range 0 &#x2264; S &#x2264; 4</param>
    public void UnlockSlot(int tid, int sid)
        => this.RemoteVoidCall("UnlockSlot", tid, sid);

    /// <summary>
    /// Send a message along the (lobby) global chat.
    /// </summary>
    /// <param name="mid">The ID of the member sending the chat message.</param>
    /// <param name="msg">The message to send.</param>
    public void GlobalChat(ulong mid, string msg)
        => this.RemoteVoidCall("GlobalChat", mid, msg);

    /// <summary>
    /// Send a message along the designated team chat.
    /// </summary>
    /// <param name="mid">The ID of the member sending the chat message.</param>
    /// <param name="msg">The message to send.</param>
    public void TeamChat(ulong mid, string msg)
        => this.RemoteVoidCall("TeamChat", mid, msg);

    /// <summary>
    /// Set the lobby setting.
    /// </summary>
    /// <param name="setting">The name of the setting being set.</param>
    /// <param name="value">The value of the setting being set.</param>
    public void SetLobbySetting(string setting, string value) {
        if (this.m_isHost) {
            this.m_settings[setting] = value; // Apply locally, just in case
            this.RemoteVoidCall("SetLobbySetting", setting, value);
        }
    }

    /// <summary>
    /// Set the lobby state.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <exception cref="ArgumentException"></exception>
    public void SetLobbyState(LobbyState state) {
        if (this.m_isHost) {
            this.RemoteVoidCall("SetLobbyState", state switch {
                LobbyState.Playing => "SERVERSTATUS_PLAYING",
                LobbyState.InLobby => "SERVERSTATUS_IN_LOBBY",
                LobbyState.Starting => "SERVERSTATUS_STARTING",
                LobbyState.None => "SERVERSTATUS_NO_STATUS",
                _ => throw new ArgumentException($"Invalid lobby state value 'LobbyState.{state}'", nameof(state))
            });
        }
    }

    /// <summary>
    /// Set the capacity of axis and allied teams.
    /// </summary>
    /// <param name="newCapacity">The new capacity of the teams, where accpeted values are in the range 0 &#x2264; <b>C</b> &#x2264; 4</param>
    /// <returns>If new capacity was set, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool SetTeamsCapacity(int newCapacity) {
        
        // Input sanity check
        if (newCapacity is < 0 or > 4) {
            return false;
        }

        if (this.m_isHost) {                
            return this.RemoteCall<bool>("SetTeamsCapacity", newCapacity);
        }
        return true;
    }

    /// <summary>
    /// Start the match following the startup grace period.
    /// </summary>
    /// <param name="cancelTime">The amount of seconds the server will wait before sending back a response if the match should continue.</param>
    /// <returns>If match received server OK, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool StartMatch(double cancelTime) {

        // Send start match command
        bool wasCancelled = this.RemoteCallWithTime<bool>("StartMatch", new object[] { cancelTime }, TimeSpan.FromSeconds(cancelTime + 1));

        // Return if someone cancelled the match 
        return !wasCancelled;

    }

    /// <summary>
    /// This will halt the ongoing match startup process.
    /// </summary>
    public void CancelMatch() {

        // Send cancel match command
        this.RemoteVoidCall("CancelStartMatch");

    }

    /// <summary>
    /// This will ask all participants to launch their game.
    /// </summary>
    public void LaunchMatch() {
        if (this.m_isHost) {
            this.RemoteVoidCall("LaunchGame");
        }
    }

    /// <summary>
    /// This will request the company file of all specified members.
    /// </summary>
    /// <param name="members">The list of members to fetch ID from. If list is empty, all participant companies will be requested.</param>
    public void RequestCompanyFile(params ulong[] members) {
        if (members.Length == 0) {
            members = this.Allies.Slots.Concat(this.Axis.Slots).Filter(x => x.IsOccupied && !x.IsSelf()).Map(x => x.Occupant?.MemberID ?? 0);
        }
        for (int i = 0; i < members.Length; i++) {
            this.RemoteVoidCall("GetCompanyFile", members[i]);
        }
    }

    /// <summary>
    /// This will notify all participants the gamemode is ready for download on the server.
    /// </summary>
    public void ReleaseGamemode() {
        if (this.m_isHost) {
            this.RemoteVoidCall("ReleaseGamemode");
        }
    }

    /// <summary>
    /// This will inform all participants that their updated company files are now available for download.
    /// </summary>
    public void ReleaseResults() {
        if (this.m_isHost) {
            this.RemoteVoidCall("ReleaseResults");
        }
    }

    /// <summary>
    /// This will halt the ongoing match for all participants.
    /// </summary>
    public void HaltMatch() {
        if (this.m_isHost) {
            this.RemoteVoidCall("HaltMatch");
        }
    }

    /// <summary>
    /// This will notify participants of match status.
    /// </summary>
    public void NotifyMatch(string infoType, string infoMessage) {
        if (this.m_isHost) {
            this.RemoteVoidCall("InfoMatch", infoType, infoMessage);
        }
    }

    /// <summary>
    /// Notifies the lobby of an error that was encounted by the caller.
    /// </summary>
    /// <param name="errorType">The type of error that was encountered.</param>
    /// <param name="errorMessage">The associated message with the error.</param>
    public void NotifyError(string errorType, string errorMessage) {

        // Determine method
        var method = this.m_isHost ? "ErrorMatch" : "NotifyHostError";

        // Create message
        Message msg = new Message() {
            CID = this.m_cidcntr++,
            Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = new[] { errorType, errorMessage } }),
            Mode = MessageMode.BrokerCall,
            Sender = this.m_connection.SelfID,
            Target = 0
        };

        // Send but ignore response
        this.m_connection.SendMessage(msg);

    }

    /// <summary>
    /// Uploads the gamemode file contents to the server along the underlying TCP connection.
    /// </summary>
    /// <param name="contents">The .sga archive file contents.</param>
    /// <param name="callbackHandler">The callback that is triggered whenever a chunk is sent.</param>
    /// <returns><see langword="true"/> if file was uploaded; Otherwise <see langword="false"/>.</returns>
    public bool UploadGamemodeFile(byte[] contents, UploadProgressCallbackHandler? callbackHandler)
        => this.m_connection.SendFile(contents, 0, this.m_cidcntr++, callbackHandler);

    /// <summary>
    /// Conducts a simple yes/no poll across the server.
    /// </summary>
    /// <param name="pollType">The type of poll we are conducting</param>
    /// <param name="cancelTime">The amount of seconds the server will wait before sending back a response if the match should continue.</param>
    /// <returns>If match received server OK, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public LobbyPollResults ConductPoll(string pollType, double pollTime = 3) {

        // Conduct poll with expecnded time
        if (this.RemoteCallWithTime<LobbyPoll>("PollInfo", new object[] { pollType }, TimeSpan.FromSeconds(pollTime + 1)) is LobbyPoll poll) {

            // Count yays
            byte y = (byte)poll.Responses.Aggregate(0, (a, b) => a + (b.Value ? 1 : 0));

            // return poll result (nays computed based on yays)
            return new(y, (byte)(poll.Responses.Count - y), false);

        }

        // Return timed out
        return new(0,0,true);

    }

    /// <summary>
    /// Responds to a poll with specified <paramref name="pollVote"/>.
    /// </summary>
    /// <param name="pollId">The ID of the poll being responded to</param>
    /// <param name="pollVote">flag setting if vote is a yes or a no.</param>
    public void RespondPoll(string pollId, bool pollVote)
        => this.RemoteVoidCall("PollRespond", pollId, EncBool(pollVote));

    private static T BitConvert<T>(byte[] raw, Func<byte[], int, T> func) {
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(raw);
        }
        return func(raw, 0);
    }

    private T? RemoteCall<T>(string method, params object[] args)
        => this.RemoteCallWithTime<T>(method, args);

    private T? RemoteCallWithTime<T>(string method, object[] args, TimeSpan? waitTime = null) {
        
        // Create message
        Message msg = new Message() {
            CID = this.m_cidcntr++,
            Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = args.Map(x => x.ToString()).NotNull() }),
            Mode = MessageMode.BrokerCall,
            Sender = this.m_connection.SelfID,
            Target = 0
        };

        // Send and await response
        if (this.m_connection.SendAndAwaitReply(msg, waitTime) is ContentMessage response) {
            if (response.MessageType == ContentMessgeType.Error) {
                string e = response.StrMsg is null ? "Received error message from server with no description" : response.StrMsg;
                throw new Exception(e);
            }
            if (response.StrMsg == "Primitive") {
                // It feels nasty using 'dynamic'
                return (T)(dynamic)(response.DotNetType switch {
                    nameof(Boolean) => response.Raw[0] == 1,
                    nameof(UInt32) => BitConvert(response.Raw, BitConverter.ToUInt32),
                    _ => throw new NotImplementedException($"Support for primitive response of type '{response.DotNetType}' not implemented.")
                });
            } else {
                if (GoMarshal.JsonUnmarshal<T>(response.Raw) is T settings) {
                    if (settings is IAPIObject apiobj) {
                        apiobj.SetAPI(this);
                    }
                    return settings;
                }
            }
        }
        
        // TODO: Warning
        return default;

    }

    private void RemoteVoidCall(string method, params object[] args) {
        
        // Create message
        Message msg = new Message() {
            CID = this.m_cidcntr++,
            Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = args.Map(x => x.ToString()).NotNull() }),
            Mode = MessageMode.BrokerCall,
            Sender = this.m_connection.SelfID,
            Target = 0
        };
        
        // Send but ignore response
        this.m_connection.SendMessage(msg);

    }

}
