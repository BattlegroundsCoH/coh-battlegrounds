namespace Battlegrounds.Models.Matches;

/// <summary>
/// Represents the outcome of a match play operation, including error and replay information.
/// </summary>
/// <remarks>Use this type to inspect the result of a match play attempt, including whether it failed, error
/// details, and the location of any generated replay file. All properties are immutable and set at
/// initialization.</remarks>
public sealed class MatchPlayResult {

    /// <summary>
    /// Gets a value indicating whether the operation has failed.
    /// </summary>
    public bool Failed { get; init; }

    /// <summary>
    /// Gets the error message associated with the current operation or state.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether a scar-related error has occurred.
    /// </summary>
    public bool ScarError { get; init; }

    /// <summary>
    /// Gets a value indicating whether BugSplat crash reporting is enabled.
    /// </summary>
    public bool BugSplat { get; init; }

    /// <summary>
    /// Gets the file path to the replay file used for playback or analysis.
    /// </summary>
    public string ReplayFilePath { get; init; } = string.Empty;

}
