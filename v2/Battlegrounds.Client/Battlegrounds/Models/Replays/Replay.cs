namespace Battlegrounds.Models.Replays;

public sealed class Replay {

    public string GameId { get; init; } = string.Empty;

    public IReadOnlyList<ReplayPlayer> Players { get; init; } = [];

    public TimeSpan Duration { get; init; }

    public IReadOnlyList<ReplayEvent> Events { get; init; } = [];

}
