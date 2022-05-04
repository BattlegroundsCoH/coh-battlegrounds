using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play;

/// <summary>
/// Strategy interface for handling the play session.
/// </summary>
public interface IPlayStrategy {

    /// <summary>
    /// Get if the strategy has launched the game.
    /// </summary>
    bool IsLaunched { get; }

    /// <summary>
    /// Get the associated session for this play session.
    /// </summary>
    ISession Session { get; }

    /// <summary>
    /// Tell the strategy to launch
    /// </summary>
    void Launch();

    /// <summary>
    /// Wait for the game to exit
    /// </summary>
    void WaitForExit();

    /// <summary>
    /// Check if any errors occured (bug splat or scar error).
    /// </summary>
    /// <returns>Will return <see langword="true"/> if no fatal errors occured while playing. Otherwise <see langword="false"/>.</returns>
    bool IsPerfect();

    /// <summary>
    /// Get the results of the match. Will return <see langword="null"/> if <see cref="IsPerfect"/> is <see langword="false"/>.
    /// </summary>
    /// <returns>The retrieved <see cref="IMatchData"/> object that can be generated after the match.</returns>
    IMatchData GetResults();

}

