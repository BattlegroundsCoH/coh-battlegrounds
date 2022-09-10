using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Enum representing the various objective types available.
/// </summary>
public enum PlanningObjectiveType {
    
    /// <summary>
    /// No objective.
    /// </summary>
    None,

    /// <summary>
    /// Objective Type: Attack
    /// </summary>
    /// <remarks>
    /// Attack a position -> Capture strategic point.
    /// </remarks>
    OT_Attack,

    /// <summary>
    /// Objective Type: Defend
    /// </summary>
    /// <remarks>
    /// Defend a position -> Hold a position safely (no enemy units) for 3 minutes.
    /// </remarks>
    OT_Defend,
   
    /// <summary>
    /// Objective Type: Support
    /// </summary>
    /// <remarks>
    /// Provice support to another player's objective
    /// </remarks>
    OT_Support

}

/// <summary>
/// Interface representing a plan element.
/// </summary>
public interface ILobbyPlanElement {

    /// <summary>
    /// Get the ID identifying the element.
    /// </summary>
    int ElementId { get; }

    /// <summary>
    /// Get the owner of the element.
    /// </summary>
    ulong ElementOwnerId { get; }

    /// <summary>
    /// Get the target/spawn position of the element.
    /// </summary>
    GamePosition SpawnPosition { get; }

    /// <summary>
    /// Get the lookat/destination position of the element.
    /// </summary>
    GamePosition? LookatPosition { get; }

    /// <summary>
    /// Get if the element is an entity.
    /// </summary>
    bool IsEntity { get; }

    /// <summary>
    /// Get if the element placement is directional.
    /// </summary>
    bool IsDirectional { get; }

    /// <summary>
    /// Get the name of the blueprint of the element.
    /// </summary>
    string Blueprint { get; }

    /// <summary>
    /// Get the company ID of the unit being spawned by the element.
    /// </summary>
    ushort CompanyId { get; }

    /// <summary>
    /// Get the objective type
    /// </summary>
    PlanningObjectiveType ObjectiveType { get; }

    /// <summary>
    /// Get the order of the objective.
    /// </summary>
    int ObjectiveOrder { get; }

    /// <summary>
    /// Set the order of the element objective.
    /// </summary>
    /// <param name="order">The new order of the objective.</param>
    void SetObjectiveOrder(int order);

}
