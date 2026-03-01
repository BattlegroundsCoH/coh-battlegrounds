using Battlegrounds.Models.Statistics;

namespace Battlegrounds.Services;

/// <summary>
/// Defines the contract for a statistics service that manages and provides access to match statistics data.
/// </summary>
/// <remarks>Implementations of this interface are responsible for loading, recording, and retrieving match
/// statistics. Members are designed for asynchronous usage and may require awaiting the loading state before accessing
/// data. Thread safety and data consistency depend on the specific implementation.</remarks>
public interface IStatisticsService {

    /// <summary>
    /// Gets a task that represents the asynchronous loading state of the object.
    /// </summary>
    /// <remarks>The returned task completes when the object's loading process finishes, regardless of success
    /// or failure. Await this task to ensure the object is fully loaded before accessing dependent members.</remarks>
    Task IsLoaded { get; }

    /// <summary>
    /// Asynchronously loads the latest statistics data into the current context.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    Task LoadStatisticsAsync();

    /// <summary>
    /// Asynchronously registers a played match in the system.
    /// </summary>
    /// <param name="match">The match data to be recorded. Must not be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RegisterPlayedMatchAsync(MatchPlayed match);

    /// <summary>
    /// Retrieves a read-only list of matches that have been played.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyList{MatchPlayed}"/> containing all played matches. The list will be empty if no matches
    /// have been played.</returns>
    IReadOnlyList<MatchPlayed> GetPlayedMatches();

}
