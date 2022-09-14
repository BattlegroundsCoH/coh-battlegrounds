namespace Battlegrounds.Game.Match;

public interface ISessionPlanSquad {

    public int TeamOwner { get; }

    public int TeamMemberOwner { get; }

    public ushort SpawnId { get; }

    public GamePosition Spawn { get; }

    public GamePosition? Lookat { get; }

}
