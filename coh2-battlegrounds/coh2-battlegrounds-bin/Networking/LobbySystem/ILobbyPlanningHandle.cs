using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Interface for a handle to a lobby planning instance.
/// </summary>
public interface ILobbyPlanningHandle {

    /// <summary>
    /// Get the <see cref="ILobbyHandle"/> that handles the respective API calls.
    /// </summary>
    ILobbyHandle Handle { get; }

    /// <summary>
    /// Get if the local player is defending.
    /// </summary>
    bool IsDefender { get; }

    /// <summary>
    /// Get if the local player is attacking.
    /// </summary>
    bool IsAttacker { get; }
    
    /// <summary>
    /// Get the size of the team of the local player.
    /// </summary>
    int TeamSize { get; }

    /// <summary>
    /// Get the index of the team of the local player
    /// </summary>
    byte Team { get; }

    /// <summary>
    /// Event triggered when a plan element is added remotely.
    /// </summary>
    event LobbyEventHandler<ILobbyPlanElement>? PlanElementAdded;

    /// <summary>
    /// Event triggered when a plan element is removed remotely.
    /// </summary>
    event LobbyEventHandler<int>? PlanElementRemoved;

    /// <summary>
    /// Create a new planning structure for the specified owner.
    /// </summary>
    /// <param name="owner">The Steam Id of the owner.</param>
    /// <param name="blueprint">The entity blueprint name.</param>
    /// <param name="directional">Flag marking if the placement is directional</param>
    /// <param name="origin">The spawn position</param>
    /// <param name="lookat">The lookat position (end position if directional is <see langword="false"/>). Optional parameter.</param>
    /// <returns>The unique ID identifying the structure.</returns>
    int CreatePlanningStructure(ulong owner, string blueprint, bool directional, GamePosition origin, GamePosition? lookat = null);

    /// <summary>
    /// Create a new pre-placed squad for the specified owner.
    /// </summary>
    /// <param name="owner">The Steam Id of the owner.</param>
    /// <param name="blueprint">The squad blueprint name.</param>
    /// <param name="companyId">The id of the unit in the company of the owner.</param>
    /// <param name="spawn">The spawn position</param>
    /// <param name="lookat">Position to look at</param>
    /// <returns>The unique ID identifying the squad placement.</returns>
    int CreatePlanningSquad(ulong owner, string blueprint, ushort companyId, GamePosition spawn, GamePosition? lookat = null);

    /// <summary>
    /// Create a new planning objective element.
    /// </summary>
    /// <param name="owner">The owner of the planning objective.</param>
    /// <param name="objectiveType">The objective type.</param>
    /// <param name="objectiveOrder">The order of the objective.</param>
    /// <param name="objectivePosition">The position of the objective.</param>
    /// <returns>The unique ID identifying the planning objective.</returns>
    int CreatePlanningObjective(ulong owner, PlanningObjectiveType objectiveType, int objectiveOrder, GamePosition objectivePosition);

    /// <summary>
    /// Remove a planning
    /// </summary>
    /// <param name="planElementId">The id of the plan element to remove.</param>
    void RemovePlanElement(int planElementId);

    /// <summary>
    /// Get an array of all planning elements for the specified team. (Index of 4 = get all)
    /// </summary>
    /// <param name="teamIndex">The index of the team to retrieve planning elements from.</param>
    /// <returns>Array of plan elements.</returns>
    ILobbyPlanElement[] GetPlanningElements(byte teamIndex);

    /// <summary>
    /// Get the plan element given by the specified element id.
    /// </summary>
    /// <param name="planElementId">The id of the plan element to fetch.</param>
    /// <returns>If found, the plan element; Otherwise null.</returns>
    ILobbyPlanElement? GetPlanElement(int planElementId);

    /// <summary>
    /// Clear all plan elements
    /// </summary>
    void ClearPlan();

}
