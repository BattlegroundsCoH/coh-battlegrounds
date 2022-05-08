namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface ILobbyTeam : IHandleObject {

    /// <summary>
    /// 
    /// </summary>
    public ILobbySlot[] Slots { get; }

    /// <summary>
    /// 
    /// </summary>
    public int TeamID { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public int Capacity { get; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="memberID"></param>
    /// <returns></returns>
    public ILobbySlot? GetSlotOfMember(ulong memberID);

}
