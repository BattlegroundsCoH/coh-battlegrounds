using System.Diagnostics.CodeAnalysis;

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
    /// Asynchronously loads map data required for application functionality.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation. The task completes when all map data has been loaded.</returns>
    Task LoadMapsAsync();

    /// <summary>
    /// Asynchronously retrieves the latest map associated with the specified game.
    /// </summary>
    /// <remarks>Ensure that the provided gameId is valid and corresponds to an existing game. If the gameId
    /// is invalid, the method may throw an exception.</remarks>
    /// <param name="gameId">The unique identifier of the game for which to retrieve the latest map. This parameter cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the latest map for the specified
    /// game.</returns>
    Task<Scenario> GetLatestMapAsync(string gameId);

    /// <summary>
    /// Retrieves the map associated with the specified scenario name for the given game instance.
    /// </summary>
    /// <remarks>Ensure that the scenario name provided is valid and corresponds to an existing map within the
    /// game instance.</remarks>
    /// <param name="game">The game instance for which the scenario map is to be retrieved. This parameter must not be null.</param>
    /// <param name="mapId">The name of the scenario whose map is requested. This parameter must not be empty.</param>
    /// <returns>A Scenario object representing the scenario's map. Returns null if no map is found for the specified scenario name.</returns>
    Scenario GetMapByScenarioName(Game game, string mapId);

    /// <summary>
    /// Retrieves the scenario map associated with the specified game and map identifier, or returns null if no matching
    /// scenario is found.
    /// </summary>
    /// <param name="game">The name of the game for which to retrieve the scenario map. Cannot be null or empty.</param>
    /// <param name="mapId">The identifier of the map within the specified game. Cannot be null or empty.</param>
    /// <returns>A <see cref="Scenario"/> object representing the scenario map if found; otherwise, <see langword="null"/>.</returns>
    Scenario? GetMapByScenarioNameOrNull(string game, string mapId);

    /// <summary>
    /// Attempts to retrieve a map associated with the specified scenario name from the given game.
    /// </summary>
    /// <param name="game">The game instance from which to search for the map. Cannot be null.</param>
    /// <param name="mapId">The identifier of the scenario whose map is to be retrieved. Cannot be null or empty.</param>
    /// <param name="map">When this method returns <see langword="true"/>, contains the map associated with the specified scenario name;
    /// otherwise, contains <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a map with the specified scenario name is found; otherwise, <see langword="false"/>.</returns>
    bool TryGetMapByScenarioName(Game game, string mapId, [NotNullWhen(true)] out Scenario? map);

    /// <summary>
    /// Retrieves a collection of maps associated with the specified game identifier.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. Ensure that the provided game identifier
    /// corresponds to a valid game.</remarks>
    /// <param name="gameId">The unique identifier of the game for which to retrieve maps. This parameter cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of maps related to the
    /// specified game. The list will be empty if no maps are found.</returns>
    Task<List<Scenario>> GetMapsForGame(string gameId);

    /// <summary>
    /// Retrieves a collection of maps associated with the specified game type.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. Ensure that the game type provided is
    /// valid and has associated maps.</remarks>
    /// <typeparam name="T">The type of game for which maps are being retrieved. Must inherit from the Game class.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of Map objects associated
    /// with the specified game type.</returns>
    Task<List<Scenario>> GetMapsForGame<T>() where T : Game;

}
