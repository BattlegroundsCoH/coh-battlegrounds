using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem.Local;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public class LocalLobbyPlanner : ILobbyPlanningHandle {

    private readonly List<ILobbyPlanElement> m_elements;
    private int m_elementCounter;

    /// <summary>
    /// 
    /// </summary>
    public event LobbyEventHandler<ILobbyPlanElement>? PlanElementAdded;

    /// <summary>
    /// 
    /// </summary>
    public event LobbyEventHandler<int>? PlanElementRemoved;

    /// <summary>
    /// 
    /// </summary>
    public ILobbyHandle Handle { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsDefender => this.Handle.AreTeamRolesSwapped() ? this.Team is 0 : this.Team is 1;

    /// <summary>
    /// 
    /// </summary>
    public bool IsAttacker => !this.IsDefender;

    /// <summary>
    /// 
    /// </summary>
    public byte Team => this.Handle.GetSelfTeam();

    /// <summary>
    /// Get the size of the team the local machine is one
    /// </summary>
    public int TeamSize => (this.Team switch {
        0 => this.Handle.Allies,
        1 => this.Handle.Axis,
        _ => this.Handle.Observers
    }).Slots.Filter(x => x.IsOccupied).Length;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    public LocalLobbyPlanner(ILobbyHandle handle) {
        
        // Set handle
        this.Handle = handle;
        
        // Create list for elements
        this.m_elements = new();

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
    /// <param name="owner"></param>
    /// <param name="objectiveType"></param>
    /// <param name="objectiveOrder"></param>
    /// <param name="objectivePosition"></param>
    /// <returns></returns>
    public int CreatePlanningObjective(ulong owner, PlanningObjectiveType objectiveType, int objectiveOrder, GamePosition objectivePosition) {

        // Grab new id and increment counter
        int id = this.m_elementCounter++;

        // Add element
        this.m_elements.Add(new LocalPlanElement(id, owner, objectiveType, objectiveOrder, objectivePosition));

        // Return ID
        return id;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    public ILobbyPlanElement[] GetPlanningElements(byte teamIndex) => teamIndex switch {
        0 => this.m_elements.Where(x => this.Handle.Allies.GetSlotOfMember(x.ElementOwnerId) is not null).ToArray(),
        1 => this.m_elements.Where(x => this.Handle.Axis.GetSlotOfMember(x.ElementOwnerId) is not null).ToArray(),
        4 => this.m_elements.ToArray(),
        _ => Array.Empty<ILobbyPlanElement>()
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="planElementId"></param>
    public void RemovePlanElement(int planElementId) {
        if (this.m_elements.Find(x => x.ElementId == planElementId) is ILobbyPlanElement e)
            this.m_elements.Remove(e);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="planElementId"></param>
    /// <returns></returns>
    public ILobbyPlanElement? GetPlanElement(int planElementId)
        => this.m_elements.Find(x => x.ElementId == planElementId);

    /// <summary>
    /// 
    /// </summary>
    public void ClearPlan() {
        this.m_elements.ForEach(x => this.PlanElementRemoved?.Invoke(x.ElementId));
        this.m_elements.Clear();
    }

}
