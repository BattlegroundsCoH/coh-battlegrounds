using Battlegrounds.Models.Matches;

namespace Battlegrounds.Models.Playing;

/// <summary>
/// Represents an abstract instance of a game application, providing methods to launch the game and wait for match
/// results.
/// </summary>
/// <remarks>This class defines the contract for managing the lifecycle of a game application instance.
/// Implementations should handle launching the game with specified arguments and waiting for match completion.
/// Instances are typically used to control and monitor game execution in automated or managed environments.</remarks>
public abstract class GameAppInstance {

    /// <summary>
    /// Gets the game instance associated with the current context.
    /// </summary>
    public abstract Game Game { get; }

    /// <summary>
    /// Asynchronously launches the process with the specified command-line arguments.
    /// </summary>
    /// <param name="args">An array of command-line arguments to pass to the process. Can be empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the process was
    /// launched successfully; otherwise, <see langword="false"/>.</returns>
    public abstract Task<bool> Launch(params string[] args);

    /// <summary>
    /// Waits asynchronously for a match to be found and returns the result of the match play operation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="MatchPlayResult"/>
    /// describing the outcome of the match play. The result is never null.</returns>
    public abstract Task<MatchPlayResult> WaitForMatch();

}
