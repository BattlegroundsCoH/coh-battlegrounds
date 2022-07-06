namespace Battlegrounds.Game.Match;

public interface ISessionPlanGoal {

    byte ObjectiveTeam { get; }

    byte ObjectivePlayer { get; }

    byte ObjectiveType { get; }

    byte ObjectiveIndex { get; }

    GamePosition ObjectivePosition { get; }

}
