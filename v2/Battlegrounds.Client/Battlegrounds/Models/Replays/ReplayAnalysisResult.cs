namespace Battlegrounds.Models.Replays;

public sealed class ReplayAnalysisResult {

    public bool Failed { get; init; }

    public string GameId { get; init; } = string.Empty;

    public Replay? Replay { get; init; } = null;

}
