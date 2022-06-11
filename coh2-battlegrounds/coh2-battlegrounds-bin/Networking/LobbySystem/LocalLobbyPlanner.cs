using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem.Local;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public class LocalLobbyPlanner : ILobbyPlanningHandle {

    private readonly List<ILobbyPlanElement> m_elements;
    private readonly int m_selfTeam;
    private int m_elementCounter;

    /// <summary>
    /// 
    /// </summary>
    public ILobbyHandle Handle { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsDefender => this.Handle.AreTeamRolesSwapped() ? this.m_selfTeam is 1 : this.m_selfTeam is 0;

    /// <summary>
    /// 
    /// </summary>
    public bool IsAttacker => this.Handle.AreTeamRolesSwapped() ? this.m_selfTeam is 0 : this.m_selfTeam is 1;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    public LocalLobbyPlanner(ILobbyHandle handle) {
        
        // Set handle
        this.Handle = handle;
        
        // Create list for elements
        this.m_elements = new();

        // Grab self
        this.m_selfTeam = handle.GetSelfTeam();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="blueprint"></param>
    /// <param name="companyId"></param>
    /// <param name="spawn"></param>
    /// <param name="lookat"></param>
    /// <returns></returns>
    public int CreatePlanningSquad(ulong owner, string blueprint, ushort companyId, GamePosition spawn, GamePosition? lookat = null) {

        // Get new ID
        int id = this.m_elementCounter++;

        // Add element
        this.m_elements.Add(new LocalPlanElement(id, owner, blueprint, companyId, spawn, lookat));

        // Return ID
        return id;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="blueprint"></param>
    /// <param name="directional"></param>
    /// <param name="origin"></param>
    /// <param name="lookat"></param>
    /// <returns></returns>
    public int CreatePlanningStructure(ulong owner, string blueprint, bool directional, GamePosition origin, GamePosition? lookat = null) {

        // Get new ID
        int id = this.m_elementCounter++;

        // Add element
        this.m_elements.Add(new LocalPlanElement(id, owner, blueprint, origin, lookat, directional));

        // Return ID
        return id;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    public ILobbyPlanElement[] GetPlanningElements(byte teamIndex)
        => this.m_elements.Where(x => (teamIndex is 0 ? this.Handle.Allies : this.Handle.Axis).GetSlotOfMember(x.ElementOwnerId) is not null).ToArray();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="planElementId"></param>
    public void RemovePlanElement(int planElementId) {
        if (this.m_elements.Find(x => x.ElementId == planElementId) is ILobbyPlanElement e)
            this.m_elements.Remove(e);
    }

}
