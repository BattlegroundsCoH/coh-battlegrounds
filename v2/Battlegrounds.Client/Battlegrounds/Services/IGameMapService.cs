using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

/// <summary>
/// Defines methods for retrieving game map data based on various criteria, supporting both asynchronous and synchronous
/// access.
/// </summary>
/// <remarks>Implementations of this interface should ensure thread safety when accessing shared resources. The
/// interface enables retrieval of maps by game identifier, scenario name, or generic game type, allowing flexible
/// access patterns for different use cases.</remarks>
public interface IGameMapService {
    
    /// <summary>
    /// Asynchronously retrieves the latest map associated with the specified game.
    /// </summary>
    /// <remarks>Ensure that the provided gameId is valid and corresponds to an existing game. If the gameId
    /// is invalid, the method may throw an exception.</remarks>
    /// <param name="gameId">The unique identifier of the game for which to retrieve the latest map. This parameter cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the latest map for the specified
    /// game.</returns>
    Task<Map> GetLatestMapAsync(string gameId);
    
    /// <summary>
    /// Retrieves the map associated with the specified scenario name for the given game instance.
    /// </summary>
    /// <remarks>Ensure that the scenario name provided is valid and corresponds to an existing map within the
    /// game instance.</remarks>
    /// <param name="game">The game instance for which the scenario map is to be retrieved. This parameter must not be null.</param>
    /// <param name="newValue">The name of the scenario whose map is requested. This parameter must not be empty.</param>
    /// <returns>A Map object representing the scenario's map. Returns null if no map is found for the specified scenario name.</returns>
    Map GetMapByScenarioName(Game game, string newValue);

    /// <summary>
    /// Retrieves a collection of maps associated with the specified game identifier.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. Ensure that the provided game identifier
    /// corresponds to a valid game.</remarks>
    /// <param name="gameId">The unique identifier of the game for which to retrieve maps. This parameter cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of maps related to the
    /// specified game. The list will be empty if no maps are found.</returns>
    Task<List<Map>> GetMapsForGame(string gameId);

    /// <summary>
    /// Retrieves a collection of maps associated with the specified game type.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. Ensure that the game type provided is
    /// valid and has associated maps.</remarks>
    /// <typeparam name="T">The type of game for which maps are being retrieved. Must inherit from the Game class.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of Map objects associated
    /// with the specified game type.</returns>
    Task<List<Map>> GetMapsForGame<T>() where T : Game;

}
