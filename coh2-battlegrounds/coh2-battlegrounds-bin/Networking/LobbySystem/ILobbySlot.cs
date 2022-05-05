using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface ILobbySlot : IHandleObject {

    /// <summary>
    /// 
    /// </summary>
    public int SlotID { get; }

    /// <summary>
    /// 
    /// </summary>
    public int TeamID { get; }

    /// <summary>
    /// 
    /// </summary>
    public byte State { get; }

    /// <summary>
    /// 
    /// </summary>
    public ILobbyMember? Occupant { get; }

    /// <summary>
    /// 
    /// </summary>
    [MemberNotNullWhen(true, nameof(Occupant))]
    public bool IsOccupied { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsSelf();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsAI();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="company"></param>
    public void TrySetCompany(ILobbyCompany company);

}
