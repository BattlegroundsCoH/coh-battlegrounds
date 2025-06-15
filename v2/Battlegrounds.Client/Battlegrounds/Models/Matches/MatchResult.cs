namespace Battlegrounds.Models.Matches;

public sealed class MatchResult {

    public bool Failed { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public bool ScarError { get; init; }

    public bool BugSplat { get; init; }

    public string ReplayFilePath { get; init; } = string.Empty;

}
