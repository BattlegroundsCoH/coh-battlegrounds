namespace Battlegrounds.Game.Match.Play.Factory;

/// <summary>
/// Factory for creating the proper overwatch strategy
/// </summary>
public sealed class OverwatchStrategyFactory : IPlayStrategyFactory {

    private readonly GameCase game;

    /// <summary>
    /// Initialise a new <see cref="OverwatchStrategyFactory"/> instance.
    /// </summary>
    /// <param name="game">The game this factory will create overwatch strategies for</param>
    public OverwatchStrategyFactory(GameCase game) {
        this.game = game;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="sessionHandler"></param>
    /// <returns></returns>
    public IPlayStrategy CreateStrategy(ISession session, ISessionHandler sessionHandler) => new OverwatchStrategy(session, sessionHandler);

}
