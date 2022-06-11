using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface ILobbyPlanElement {

    /// <summary>
    /// 
    /// </summary>
    int ElementId { get; }

    /// <summary>
    /// 
    /// </summary>
    ulong ElementOwnerId { get; }

    /// <summary>
    /// 
    /// </summary>
    GamePosition SpawnPosition { get; }

    /// <summary>
    /// 
    /// </summary>
    GamePosition LookatPosition { get; }

    /// <summary>
    /// 
    /// </summary>
    bool IsEntity { get; }

    /// <summary>
    /// 
    /// </summary>
    bool IsDirectional { get; }

    /// <summary>
    /// 
    /// </summary>
    string Blueprint { get; }

    /// <summary>
    /// 
    /// </summary>
    ushort CompanyId { get; }

}
