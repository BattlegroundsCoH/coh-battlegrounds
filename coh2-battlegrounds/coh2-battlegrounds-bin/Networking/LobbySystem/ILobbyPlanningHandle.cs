using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem;

public interface ILobbyPlanningHandle {

    /// <summary>
    /// 
    /// </summary>
    ILobbyHandle Handle { get; }

    /// <summary>
    /// 
    /// </summary>
    bool IsDefender { get; }

    /// <summary>
    /// 
    /// </summary>
    bool IsAttacker { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="blueprint"></param>
    /// <param name="origin"></param>
    /// <param name="lookat"></param>
    /// <returns></returns>
    int CreatePlanningStructure(ulong owner, string blueprint, bool directional, GamePosition origin, GamePosition? lookat = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="blueprint"></param>
    /// <param name="companyId"></param>
    /// <param name="spawn"></param>
    /// <param name="lookat"></param>
    /// <returns></returns>
    int CreatePlanningSquad(ulong owner, string blueprint, ushort companyId, GamePosition spawn, GamePosition? lookat = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="planElementId"></param>
    void RemovePlanElement(int planElementId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    ILobbyPlanElement[] GetPlanningElements(byte teamIndex);

}
