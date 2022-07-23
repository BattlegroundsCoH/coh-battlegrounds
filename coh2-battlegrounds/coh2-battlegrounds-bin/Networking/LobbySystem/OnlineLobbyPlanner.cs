using System;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.LobbySystem.json;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Util;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Class representing a planning instance for an online lobby. Implements <see cref="ILobbyPlanningHandle"/>.
/// </summary>
public sealed class OnlineLobbyPlanner : ILobbyPlanningHandle {

    private readonly RemoteCall<ILobbyHandle> m_remote;

    /// <summary>
    /// Get the <see cref="ILobbyHandle"/> associated with this planning instnace.
    /// </summary>
    public ILobbyHandle Handle { get; }

    /// <summary>
    /// Get if the local player is a defender.
    /// </summary>
    public bool IsDefender => this.Handle.AreTeamRolesSwapped() ? this.Team is 0 : this.Team is 1;

    /// <summary>
    /// Get if the local player is an attacker.
    /// </summary>
    public bool IsAttacker => !this.IsDefender;

    /// <summary>
    /// Get the size of the team of the local player.
    /// </summary>
    public int TeamSize => (this.Team switch {
        0 => this.Handle.Allies,
        1 => this.Handle.Axis,
        _ => this.Handle.Observers
    }).Slots.Filter(x => x.IsOccupied).Length;

    /// <summary>
    /// Get the index of the local player's team.
    /// </summary>
    public byte Team => this.Handle.GetSelfTeam();

    public event LobbyEventHandler<ILobbyPlanElement>? PlanElementAdded;
    public event LobbyEventHandler<int>? PlanElementRemoved;

    /// <summary>
    /// Initialise a new <see cref="OnlineLobbyPlanner"/> instance associated with the <paramref name="handle"/>.
    /// </summary>
    /// <param name="handle">The handle to the active lobby instance.</param>
    /// <param name="remote">The remote calling instance that handles remote calls to the server.</param>
    internal OnlineLobbyPlanner(ILobbyHandle handle, RemoteCall<ILobbyHandle> remote) {

        // Set handle
        this.Handle = handle;

        // Subscribe
        this.Handle.Subscribe("Notify.PlanElementAdd", this.OnElementAdd);
        this.Handle.Subscribe("Notify.PlanElementRemove", this.OnElementRemove);

        // Set connection
        this.m_remote = remote;

    }

    private void OnElementRemove(ContentMessage msg) {

        // Grab element id
        uint elemId = msg.Raw.ConvertBigEndian(BitConverter.ToUInt32);

        // Notify
        this.PlanElementRemoved?.Invoke((int)elemId);

    }

    private void OnElementAdd(ContentMessage msg) {

        // Try unmarshall
        if (GoMarshal.JsonUnmarshal<JsonPlanElement>(msg.Raw) is JsonPlanElement planElem) {
            
            // Set handles
            planElem.SetHandle(this.Handle);
            planElem.Remote = this.m_remote;

            // Invoke event
            this.PlanElementAdded?.Invoke(planElem);

        }

    }

    public void ClearPlan() {
        if (this.Handle.IsHost) {
            this.m_remote.Call("ClearPlan");
        }
    }

    public int CreatePlanningObjective(ulong owner, PlanningObjectiveType objectiveType, int objectiveOrder, GamePosition objectivePosition)
        => (int)this.m_remote.Call<uint>("CreatePlanningObjective", owner, (byte)objectiveOrder, objectiveOrder, objectivePosition);

    public int CreatePlanningSquad(ulong owner, string blueprint, ushort companyId, GamePosition spawn, GamePosition? lookat = null) => (int)(lookat switch {
        GamePosition look => this.m_remote.Call<uint>("CreatePlanningSquad", owner, blueprint, companyId, spawn, look),
        _ => this.m_remote.Call<uint>("CreatePlanningSquad", owner, blueprint, companyId, spawn)
    });

    public int CreatePlanningStructure(ulong owner, string blueprint, bool directional, GamePosition origin, GamePosition? lookat = null) => (int)(lookat switch {
        GamePosition look => this.m_remote.Call<uint>("CreatePlanningStructure", owner, blueprint, directional ? 1 : 0, origin, look),
        _ => this.m_remote.Call<uint>("CreatePlanningStructure", owner, blueprint, directional ? 1 : 0, origin)
    });

    public ILobbyPlanElement? GetPlanElement(int planElementId) => this.m_remote.Call<ILobbyPlanElement>("GetPlanElement", planElementId);

    public ILobbyPlanElement[] GetPlanningElements(byte teamIndex)
        => this.m_remote.Call<ILobbyPlanElement[]>("GetPlanElements", teamIndex) ?? Array.Empty<ILobbyPlanElement>();

    public void RemovePlanElement(int planElementId)
        => this.m_remote.Call("RemovePlanElement", planElementId);

}
