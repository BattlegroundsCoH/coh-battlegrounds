namespace Battlegrounds.Game.Match.Play.Factory;

public class BattleSimulatorStrategyFactory : IPlayStrategyFactory {

    public IPlayStrategy CreateStrategy(ISession session) => new BattleSimulatorStrategy(session);

}
