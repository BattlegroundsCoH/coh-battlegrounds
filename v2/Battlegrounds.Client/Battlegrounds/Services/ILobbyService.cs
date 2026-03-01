using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

/// <summary>
/// Defines the contract for managing game lobbies, including creation, joining, leaving, and querying available lobbies
/// and server status.
/// </summary>
/// <remarks>Implementations of this interface provide methods for interacting with multiplayer game lobbies. The
/// interface supports both single-player and multiplayer scenarios, and allows clients to discover, join, and leave
/// lobbies. Thread safety and connection management are implementation-dependent and should be considered when using
/// concrete implementations.</remarks>
public interface ILobbyService {

    /// <summary>
    /// Gets a value indicating whether there is an active lobby currently available.
    /// </summary>
    bool HasActiveLobby { get; }

    /// <summary>
    /// Gets the currently active lobby, if one is available.
    /// </summary>
    /// <remarks>The active lobby represents the session or room that the user is currently participating in.
    /// If no lobby is active, this property returns null.</remarks>
    ILobby? ActiveLobby { get; }

    /// <summary>
    /// Creates a new game lobby with the specified name, password, and game settings.
    /// </summary>
    /// <param name="name">The display name for the lobby. Must be unique among active lobbies.</param>
    /// <param name="password">An optional password required to join the lobby. Specify <see langword="null"/> or an empty string for no
    /// password.</param>
    /// <param name="multiplayer">Indicates whether the lobby is intended for multiplayer games. <see langword="true"/> for multiplayer;
    /// otherwise, <see langword="false"/>.</param>
    /// <param name="game">The game configuration to associate with the lobby. Cannot be <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ILobby"/> instance
    /// representing the newly created lobby.</returns>
    Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer, Game game);

    /// <summary>
    /// Attempts to join the specified lobby for the given game asynchronously, using an optional password if required.
    /// </summary>
    /// <param name="lobby">The lobby to join. Must represent a valid, joinable lobby instance.</param>
    /// <param name="game">The game context for which the lobby is being joined.</param>
    /// <param name="password">The password required to join the lobby, if applicable; otherwise, <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous join operation. The result is an <see cref="ILobby"/> instance if the
    /// join succeeds; otherwise, <see langword="null"/> if the join fails or the lobby is unavailable.</returns>
    Task<ILobby?> JoinLobbyAsync(BrowserLobby lobby, Game game, string? password = null);

    /// <summary>
    /// Asynchronously leaves the specified lobby, removing the current user from its participant list.
    /// </summary>
    /// <param name="lobby">The lobby instance to leave. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous leave operation. The task completes when the user has been removed from
    /// the lobby.</returns>
    Task LeaveLobbyAsync(ILobby lobby);

    /// <summary>
    /// Asynchronously determines whether the server is currently available for requests.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the server is
    /// available; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsServerAvailableAsync();

    /// <summary>
    /// Asynchronously retrieves a collection of available browser lobbies.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
    /// cref="BrowserLobby"/> objects representing the available lobbies. The collection will be empty if no lobbies are
    /// found.</returns>
    Task<IEnumerable<BrowserLobby>> GetLobbiesAsync();

}
