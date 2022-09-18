using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Match;

public interface ISessionPlanEntity {

    public int TeamOwner { get; }

    public int TeamMemberOwner { get; }

    public EntityBlueprint Blueprint { get; }

    public GamePosition Spawn { get; }

    public GamePosition? Lookat { get; }

    public bool IsDirectional { get; }

    public int Width { get; }

}
