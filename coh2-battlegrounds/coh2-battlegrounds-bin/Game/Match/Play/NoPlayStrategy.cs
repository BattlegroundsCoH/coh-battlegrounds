using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play;

public class NoPlayStrategy : IPlayStrategy {

    public bool IsLaunched => false;

    public ISession Session => new NullSession();

    public IMatchData GetResults() => new NoMatchData();

    public bool IsPerfect() => false;

    public void Launch() {}

    public void WaitForExit() {}

}
