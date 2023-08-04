using System.Threading.Tasks;

using Battlegrounds.Compiler;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Interface for a session handler that handles the compilation of a session and following up on the played session.
/// </summary>
public interface ISessionHandler {

    /// <summary>
    /// Fully compile a <see cref="Session"/> into a .sga file using a concrete <see cref="ISessionCompiler"/> instance.
    /// </summary>
    /// <param name="compiler">The <see cref="ISessionCompiler"/> compiler to use when compiling session data.</param>
    /// <param name="session">The <see cref="ISession"/> instance to compile</param>
    /// <returns>Will return <see langword="true"/> if <see cref="ISession"/> was compiled into a .sga file. Otherwise <see langword="false"/>.</returns>
    bool CompileSession(ISessionCompiler compiler, ISession session);

    /// <summary>
    /// Check the latest log if any fatal scar error occured.
    /// </summary>
    /// <returns><see langword="true"/> if a fatal scar error was detected; Otherwise <see langword="false"/>.</returns>
    bool GotFatalScarError();

    /// <summary>
    /// Check the latest log if any match was played.
    /// </summary>
    /// <returns><see langword="true"/> if a match was played; Otherwise <see langword="false"/>.</returns>
    bool WasAnyMatchPlayed();

    /// <summary>
    /// Check if a bugsplat occured.
    /// </summary>
    /// <returns><see langword="true"/> if a bug splat was detected; Otherwise <see langword="false"/>.</returns>
    Task<bool> GotBugsplat();

    /// <summary>
    /// Determine if there is a playback at the expected location
    /// </summary>
    /// <returns>Returns <see langword="true"/> if there's a playback file. Otherwise <see langword="false"/>.</returns>
    bool HasPlayback();

    /// <summary>
    /// Creates a new game process
    /// </summary>
    /// <returns>The game process</returns>
    GameProcess GetNewGameProcess();

}
