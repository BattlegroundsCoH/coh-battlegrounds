using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Readonly struct representing a planned entity.
/// </summary>
public readonly struct SessionPlanEntityInfo : ISessionPlanEntity {

    /// <summary>
    /// Get or initialise the ID of the team owner. -1 = World Owned.
    /// </summary>
    public int TeamOwner { get; init; }

    /// <summary>
    /// Get or initialise the ID of the team owner.
    /// </summary>
    public int TeamMemberOwner { get; init; }

    /// <summary>
    /// Get or initialise the blueprint to spawn.
    /// </summary>
    public EntityBlueprint Blueprint { get; init; }

    /// <summary>
    /// Get or initialise the world position to spawn the entity at.
    /// </summary>
    public GamePosition Spawn { get; init; }

    /// <summary>
    /// Get or initialise the position the spawned entity should look at.
    /// </summary>
    public GamePosition? Lookat { get; init; }

    /// <summary>
    /// Get or initialise if the placement is directional. 
    /// If <see cref="Lookat"/> is not null and <see cref="IsDirectional"/> is false; the entity will spawn as lines.
    /// </summary>
    public bool IsDirectional { get; init; }

    /// <summary>
    /// Get or initialise the width of the entity.
    /// </summary>
    public int Width { get; init; }

}

/// <summary>
/// Readonly struct representing a planned squad.
/// </summary>
public readonly struct SessionPlanSquadInfo : ISessionPlanSquad {

    /// <summary>
    /// Get or initialise the ID of the team owner.
    /// </summary>
    public int TeamOwner { get; init; }

    /// <summary>
    /// Get or initialise the ID of the team owner.
    /// </summary>
    public int TeamMemberOwner { get; init; }

    /// <summary>
    /// Get or initialise the company ID to spawn
    /// </summary>
    public ushort SpawnId { get; init; }

    /// <summary>
    /// Get or initialise the world position to spawn the squad at.
    /// </summary>
    public GamePosition Spawn { get; init; }

    /// <summary>
    /// Get or initialise the position the spawned squad should look at.
    /// </summary>
    public GamePosition? Lookat { get; init; }

}

/// <summary>
/// Readonly struct representing a plannged goal.
/// </summary>
public readonly struct SessionPlanGoalInfo : ISessionPlanGoal {

    /// <summary>
    /// Get or initialise the team this goal is for.
    /// </summary>
    public byte ObjectiveTeam { get; init; }

    /// <summary>
    /// Get or initialise the player this objective is for.
    /// </summary>
    public byte ObjectivePlayer { get; init; }

    /// <summary>
    /// Get or initialise the type of objective (1 = attack, 2 = defend, 3 = support)
    /// </summary>
    public byte ObjectiveType { get; init; }

    /// <summary>
    /// Get or initialise the index of the objective (local to user)
    /// </summary>
    public byte ObjectiveIndex { get; init; }

    /// <summary>
    /// Get or initialise the position of the objective
    /// </summary>
    public GamePosition ObjectivePosition { get; init; }

}
