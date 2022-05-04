namespace Battlegrounds.Game.Match.Play;

/// <summary>
/// Interface for a factory constructing a <see cref="IPlayStrategy"/>.
/// </summary>
public interface IPlayStrategyFactory {

    /// <summary>
    /// Create the concrete <see cref="IPlayStrategy"/> to use when playing a game.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/> to assign to the <see cref="IPlayStrategy"/>.</param>
    /// <returns>A concrete <see cref="IPlayStrategy"/>.</returns>
    IPlayStrategy CreateStrategy(ISession session);

}
