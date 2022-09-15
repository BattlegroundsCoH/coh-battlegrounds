using System.Collections.Generic;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Interface representing a handle to a lobby instnace.
/// </summary>
public interface ILobbyHandle {

    /// <summary>
    /// Get the title of the lobby.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Get if the local machine is the host of the lobby.
    /// </summary>
    bool IsHost { get; }

    /// <summary>
    /// Get the user represented in the lobby from this machine.
    /// </summary>
    SteamUser Self { get; }

    /// <summary>
    /// Get the allies team.
    /// </summary>
    ILobbyTeam Allies { get; }

    /// <summary>
    /// Get the axis team.
    /// </summary>
    ILobbyTeam Axis { get; }

    /// <summary>
    /// Get the observer team.
    /// </summary>
    ILobbyTeam Observers { get; }

    /// <summary>
    /// Get the handle to the planning instance.
    /// </summary>
    ILobbyPlanningHandle? PlanningHandle { get; }

    /// <summary>
    /// Get the settings of the lobby.
    /// </summary>
    Dictionary<string, string> Settings { get; }

    /// <summary>
    /// Event triggered when a lobby team instance is changed.
    /// </summary>
    event LobbyEventHandler<ILobbyTeam>? OnLobbyTeamUpdate;

    /// <summary>
    /// Event triggered when a lobby slot isntance is changed.
    /// </summary>
    event LobbyEventHandler<ILobbySlot>? OnLobbySlotUpdate;

    /// <summary>
    /// Event triggered when a lobby company is changed.
    /// </summary>
    event LobbyEventHandler<LobbyCompanyChangedEventArgs>? OnLobbyCompanyUpdate;

    /// <summary>
    /// Event triggered when the lobby settings have been changed.
    /// </summary>
    event LobbyEventHandler<LobbySettingsChangedEventArgs>? OnLobbySettingUpdate;

    /// <summary>
    /// Close the handle by disconnecting from the lobby instance.
    /// </summary>
    void CloseHandle();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="humansOnly"></param>
    /// <returns></returns>
    uint GetPlayerCount(bool humansOnly = false);

    /// <summary>
    /// Get the team the local machine is a member of.
    /// </summary>
    /// <returns>A byte value telling which team the local machine is a member  of.</returns>
    byte GetSelfTeam();

    /// <summary>
    /// Get if the <paramref name="memberId"/> is on the specified team.
    /// </summary>
    /// <param name="tid">The team ID to check.</param>
    /// <param name="memberId">The ID of the member to check.</param>
    /// <returns></returns>
    bool TeamHasMember(byte tid, ulong memberId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    /// <param name="company"></param>
    void SetCompany(int tid, int sid, ILobbyCompany company);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mid"></param>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    void MoveSlot(ulong mid, int tid, int sid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mid"></param>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    /// <param name="state"></param>
    void MemberState(ulong mid, int tid, int sid, LobbyMemberState state);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    /// <param name="difficulty"></param>
    /// <param name="company"></param>
    void AddAI(int tid, int sid, int difficulty, ILobbyCompany company);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    void RemoveOccupant(int tid, int sid);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    void LockSlot(int tid, int sid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    void UnlockSlot(int tid, int sid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="senderID"></param>
    /// <param name="message"></param>
    void SendChatMessage(int filter, ulong senderID, string message);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="value"></param>
    void SetLobbySetting(string setting, string value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    void SetLobbyState(LobbyState state);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="newCapacity"></param>
    /// <returns></returns>
    bool SetTeamsCapacity(int newCapacity);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team1"></param>
    /// <param name="team2"></param>
    void SetTeamRoles(string team1, string team2);

    /// <summary>
    /// 
    /// </summary>
    void SwapTeamRoles();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool AreTeamRolesSwapped();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancelTime"></param>
    /// <returns></returns>
    bool StartMatch(double cancelTime);
    
    /// <summary>
    /// 
    /// </summary>
    void CancelMatch();
    
    /// <summary>
    /// 
    /// </summary>
    void LaunchMatch();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="members"></param>
    void RequestCompanyFile(params ulong[] members);
    
    /// <summary>
    /// 
    /// </summary>
    void ReleaseGamemode();
    
    /// <summary>
    /// 
    /// </summary>
    void ReleaseResults();
    
    /// <summary>
    /// 
    /// </summary>
    void HaltMatch();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="infoType"></param>
    /// <param name="infoMessage"></param>
    void NotifyMatch(string infoType, string infoMessage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errorType"></param>
    /// <param name="errorMessage"></param>
    void NotifyError(string errorType, string errorMessage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="screen"></param>
    void NotifyScreen(string screen);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    UploadResult UploadGamemodeFile(byte[] contents, UploadProgressCallbackHandler? callbackHandler);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="companyOwner"></param>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    UploadResult UploadCompanyFile(byte[] contents, ulong companyOwner, UploadProgressCallbackHandler? callbackHandler);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pollType"></param>
    /// <param name="pollTime"></param>
    /// <returns></returns>
    LobbyPollResults ConductPoll(string pollType, double pollTime = 3);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pollId"></param>
    /// <param name="pollVote"></param>
    void RespondPoll(string pollId, bool pollVote);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="to"></param>
    /// <param name="eventHandler"></param>
    void Subscribe(string to, LobbyEventHandler<ContentMessage> eventHandler);

}
