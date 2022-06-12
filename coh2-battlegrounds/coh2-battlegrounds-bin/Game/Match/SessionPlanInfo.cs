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

}

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

public readonly struct SessionPlanGoalInfo : ISessionPlanGoal {

}
