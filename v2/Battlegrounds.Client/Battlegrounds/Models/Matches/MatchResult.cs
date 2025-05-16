namespace Battlegrounds.Models.Matches;

public sealed class MatchResult {

    public bool Failed { get; }

    public bool ScarError { get; }

    public bool BugSplat { get; }

    public string ReplayFilePath { get; }

}
