namespace Battlegrounds.Game.Match.Play.Factory;

public class OverwatchStrategyFactory : IPlayStrategyFactory {
    public IPlayStrategy CreateStrategy(ISession session) => new OverwatchStrategy(session);
}
