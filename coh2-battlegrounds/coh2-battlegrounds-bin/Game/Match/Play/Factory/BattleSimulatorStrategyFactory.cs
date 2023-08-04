namespace Battlegrounds.Game.Match.Play.Factory;

public sealed class BattleSimulatorStrategyFactory : IPlayStrategyFactory {

    public IPlayStrategy CreateStrategy(ISession session, ISessionHandler sessionHandler) => new BattleSimulatorStrategy(session);

}
