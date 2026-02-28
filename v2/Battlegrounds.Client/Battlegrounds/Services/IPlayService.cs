using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

/// <summary>
/// Defines methods for building a game mode based on a specified lobby and for launching a game application.
/// </summary>
/// <remarks>Implementations of this interface are responsible for preparing the game environment and initiating
/// the game application according to the provided parameters. Methods are asynchronous and return results that indicate
/// the outcome of the requested operations.</remarks>
public interface IPlayService {
    
    /// <summary>
    /// Asynchronously builds the game mode using the specified lobby configuration and state.
    /// </summary>
    /// <remarks>This method may perform network operations and should be awaited to ensure the build process
    /// completes before proceeding. The caller is responsible for handling the result to determine whether the game
    /// mode was built successfully.</remarks>
    /// <param name="lobby">The lobby instance that provides the configuration and current state required to build the game mode. Cannot be
    /// null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a BuildGamemodeResult that indicates
    /// the outcome of the build process.</returns>
    Task<BuildGamemodeResult> BuildGamemode(ILobby lobby);
    
    /// <summary>
    /// Launches the specified game application asynchronously and returns the result of the launch operation.
    /// </summary>
    /// <remarks>Ensure that the game is properly configured before calling this method. The method may throw
    /// exceptions if the game cannot be launched due to invalid settings or other issues.</remarks>
    /// <param name="game">The game instance to be launched. This parameter must not be null and should represent a valid game
    /// configuration.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the game
    /// launch was successful or failed.</returns>
    Task<LaunchGameAppResult> LaunchGameApp(Game game);

    /// <summary>
    /// Ensures that the mod source is available for use, performing any necessary checks or setup asynchronously.
    /// Fetches the wincondition source files from the server if they are not already present locally. This method should be called before attempting to build or launch a game mode that relies on the mod source.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task completes when the mod source is confirmed to be
    /// available.</returns>
    Task EnsureModSourceIsAvailable();

}
