using System;
using System.Text.Json.Serialization;

using Battlegrounds.Game;
using Battlegrounds.Networking.Remoting;

namespace Battlegrounds.Networking.LobbySystem.json;

public class JsonPlanElement : ILobbyPlanElement, IHandleObject<ILobbyHandle> {

    private ILobbyHandle? m_handle;

    public int ElementId { get; init; }

    public ulong ElementOwnerId { get; init; }

    public GamePosition SpawnPosition { get; init; }

    public GamePosition? LookatPosition { get; init; }

    public bool IsEntity { get; init; }

    public bool IsDirectional { get; init; }

    public string Blueprint { get; init; } = string.Empty;

    public ushort CompanyId { get; init; }

    public PlanningObjectiveType ObjectiveType { get; init; }

    public int ObjectiveOrder { get; private set; }

    [JsonIgnore]
    public ILobbyHandle Handle => this.m_handle ?? throw new Exception("No lobby handle assigned to plan element");

    [JsonIgnore]
    internal RemoteCall<ILobbyHandle>? Remote { get; set; }

    public void SetHandle(ILobbyHandle handle) {
        this.m_handle = handle;
    }

    public void SetObjectiveOrder(int order) {
        this.ObjectiveOrder = order;
        this.Remote?.Call("SetObjectiveOrder", this.ElementId, order);
    }

}
