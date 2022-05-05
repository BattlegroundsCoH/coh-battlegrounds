using System.Collections.Generic;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface ILobbyHandle {

    /// <summary>
    /// 
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsHost { get; }

    /// <summary>
    /// 
    /// </summary>
    public SteamUser Self { get; }

    /// <summary>
    /// 
    /// </summary>
    public ILobbyTeam Allies { get; }

    /// <summary>
    /// 
    /// </summary>
    public ILobbyTeam Axis { get; }

    /// <summary>
    /// 
    /// </summary>
    public ILobbyTeam Observers { get; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, string> Settings { get; }

    /// <summary>
    /// 
    /// </summary>
    public event LobbyEventHandler<ILobbyMessage>? OnChatMessage;

    /// <summary>
    /// Event triggered when the system announces a message.
    /// </summary>
    public event LobbyEventHandler<LobbySystemMessageEventArgs>? OnSystemMessage;

    /// <summary>
    /// Event triggered when a lobby team instance is changed.
    /// </summary>
    public event LobbyEventHandler<ILobbyTeam>? OnLobbyTeamUpdate;

    /// <summary>
    /// Event triggered when a lobby slot isntance is changed.
    /// </summary>
    public event LobbyEventHandler<ILobbySlot>? OnLobbySlotUpdate;

    /// <summary>
    /// Event triggered when the host has pressed the "begin match" button.
    /// </summary>
    public event LobbyEventHandler? OnLobbyBeginMatch;

    /// <summary>
    /// Event triggered when the game should be launched.
    /// </summary>
    public event LobbyEventHandler? OnLobbyLaunchGame;

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
    /// 
    /// </summary>
    public void CloseHandle();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="humansOnly"></param>
    /// <returns></returns>
    public uint GetPlayerCount(bool humansOnly = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    /// <param name="company"></param>
    public void SetCompany(int tid, int sid, ILobbyCompany company);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mid"></param>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    public void MoveSlot(ulong mid, int tid, int sid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mid"></param>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    /// <param name="state"></param>
    public void MemberState(ulong mid, int tid, int sid, LobbyMemberState state);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    /// <param name="difficulty"></param>
    /// <param name="company"></param>
    public void AddAI(int tid, int sid, int difficulty, ILobbyCompany company);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    public void RemoveOccupant(int tid, int sid);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    public void LockSlot(int tid, int sid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="sid"></param>
    public void UnlockSlot(int tid, int sid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="senderID"></param>
    /// <param name="message"></param>
    public void SendChatMessage(int filter, ulong senderID, string message);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="value"></param>
    public void SetLobbySetting(string setting, string value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void SetLobbyState(LobbyState state);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="newCapacity"></param>
    /// <returns></returns>
    public bool SetTeamsCapacity(int newCapacity);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancelTime"></param>
    /// <returns></returns>
    bool StartMatch(double cancelTime);
    
    /// <summary>
    /// 
    /// </summary>
    public void CancelMatch();
    
    /// <summary>
    /// 
    /// </summary>
    public void LaunchMatch();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="members"></param>
    public void RequestCompanyFile(params ulong[] members);
    
    /// <summary>
    /// 
    /// </summary>
    public void ReleaseGamemode();
    
    /// <summary>
    /// 
    /// </summary>
    public void ReleaseResults();
    
    /// <summary>
    /// 
    /// </summary>
    public void HaltMatch();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="infoType"></param>
    /// <param name="infoMessage"></param>
    public void NotifyMatch(string infoType, string infoMessage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errorType"></param>
    /// <param name="errorMessage"></param>
    public void NotifyError(string errorType, string errorMessage);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    public UploadResult UploadGamemodeFile(byte[] contents, UploadProgressCallbackHandler? callbackHandler);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="companyOwner"></param>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    public UploadResult UploadCompanyFile(byte[] contents, ulong companyOwner, UploadProgressCallbackHandler? callbackHandler);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pollType"></param>
    /// <param name="pollTime"></param>
    /// <returns></returns>
    public LobbyPollResults ConductPoll(string pollType, double pollTime = 3);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pollId"></param>
    /// <param name="pollVote"></param>
    public void RespondPoll(string pollId, bool pollVote);

}
