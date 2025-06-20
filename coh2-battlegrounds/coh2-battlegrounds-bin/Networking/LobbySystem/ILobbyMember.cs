﻿namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public enum LobbyMemberState : byte {
    
    /// <summary>
    /// 
    /// </summary>
    Joining = 0,
    
    /// <summary>
    /// 
    /// </summary>
    Waiting = 1,

    /// <summary>
    /// 
    /// </summary>
    EditCompany = 2,

    /// <summary>
    /// 
    /// </summary>
    Planning = 3,

}

/// <summary>
/// 
/// </summary>
public interface ILobbyMember : IHandleObject<ILobbyHandle> {

    /// <summary>
    /// 
    /// </summary>
    public ulong MemberID { get; }

    /// <summary>
    /// 
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 
    /// </summary>
    public byte Role { get; }

    /// <summary>
    /// 
    /// </summary>
    public byte AILevel { get; }

    /// <summary>
    /// 
    /// </summary>
    public LobbyMemberState State { get; }

    /// <summary>
    /// 
    /// </summary>
    public ILobbyCompany? Company { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="company"></param>
    public void ChangeCompany(ILobbyCompany? company);

}
