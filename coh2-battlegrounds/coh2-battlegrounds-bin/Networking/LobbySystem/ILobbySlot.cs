using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Game;

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
    public byte State { get; set; }

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
    /// <param name="minDifficulty"></param>
    /// <param name="maxDifficulty"></param>
    /// <returns></returns>
    public bool IsAI(AIDifficulty minDifficulty, AIDifficulty maxDifficulty = AIDifficulty.AI_Expert);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="company"></param>
    public void TrySetCompany(ILobbyCompany company);
    
}
