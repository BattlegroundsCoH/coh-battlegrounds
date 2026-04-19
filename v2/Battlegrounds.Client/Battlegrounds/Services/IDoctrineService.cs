using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Doctrines;

namespace Battlegrounds.Services;

/// <summary>
/// Defines a service for loading and retrieving doctrine definitions.
/// </summary>
public interface IDoctrineService {

    /// <summary>
    /// Gets a task that completes when doctrines have been loaded.
    /// </summary>
    Task DoctrinesLoaded { get; }

    /// <summary>
    /// Retrieves a doctrine definition by its unique identifier.
    /// </summary>
    /// <param name="identifier">The unique identifier of the doctrine to retrieve.</param>
    /// <returns>The doctrine definition with the specified identifier.</returns>
    DoctrineDefinition GetDoctrineById(string identifier);

    /// <summary>
    /// Attempts to retrieve a doctrine definition by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the doctrine to retrieve.</param>
    /// <param name="doctrine">When this method returns true, contains the doctrine definition; otherwise, null.</param>
    /// <returns>true if the doctrine was found; otherwise, false.</returns>
    bool TryGetDoctrineById(string identifier, [NotNullWhen(true)] out DoctrineDefinition? doctrine);

    /// <summary>
    /// Retrieves the doctrine definitions for a specific faction in a game.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="faction">The faction name.</param>
    /// <returns>A collection of doctrine definitions for the specified faction.</returns>
    IEnumerable<DoctrineDefinition> GetDoctrinesForFaction(string gameId, string faction);

    /// <summary>
    /// Gets the base doctrine definition for the specified game and faction.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="faction">The faction identifier.</param>
    /// <returns>The base doctrine definition for the specified game and faction.</returns>
    DoctrineDefinition GetBaseDoctrine(string gameId, string faction);

    /// <summary>
    /// Loads doctrines.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of doctrines loaded.</returns>
    Task<int> LoadDoctrines(CancellationToken cancellationToken);

}
