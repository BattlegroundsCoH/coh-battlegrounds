using Battlegrounds.Models.Matches;

namespace Battlegrounds.Models.Playing;

public abstract class GameAppInstance {

    public abstract Game Game { get; }

    public abstract Task<bool> Launch(params string[] args);

    public abstract Task<MatchResult> WaitForMatch();

}
