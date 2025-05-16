using Battlegrounds.Models.Matches;

namespace Battlegrounds.Models.Playing;

public abstract class GameAppInstance {

    public abstract Game Game { get; }

    public async Task<MatchResult> WaitForMatch() {
        throw new NotImplementedException();
    }

}
