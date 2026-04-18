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
    /// Loads doctrines.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of doctrines loaded.</returns>
    Task<int> LoadDoctrines(CancellationToken cancellationToken);

}
