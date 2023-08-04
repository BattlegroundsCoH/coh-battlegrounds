namespace Battlegrounds.Game.Match.Play;

/// <summary>
/// Interface for a factory constructing a <see cref="IPlayStrategy"/>.
/// </summary>
public interface IPlayStrategyFactory {

    /// <summary>
    /// Create the concrete <see cref="IPlayStrategy"/> to use when playing a game.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/> to assign to the <see cref="IPlayStrategy"/>.</param>
    /// <param name="sessionHandler">The <see cref="ISessionHandler"/> to use when handling the play session.</param>
    /// <returns>A concrete <see cref="IPlayStrategy"/>.</returns>
    IPlayStrategy CreateStrategy(ISession session, ISessionHandler sessionHandler);

}
