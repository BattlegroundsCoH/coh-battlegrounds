using Battlegrounds.Game.Match.Data;
using Battlegrounds.Logging;

namespace Battlegrounds.Game.Match.Play;

/// <summary>
/// Play strategy for players who do not have full control over the match being played by some remote <see cref="OverwatchStrategy"/>.
/// </summary>
public class MemberOverwatchStrategy : IPlayStrategy {

    private static readonly Logger logger = Logger.CreateLogger();

    private bool m_isLaunched;
    private IMatchData m_result;
    private readonly GameProcess m_process;

    public MemberOverwatchStrategy() {
        this.m_isLaunched = false;
        this.m_result = new NoMatchData();
        this.m_process = new CoH2Process(); // Temp
        logger.Debug("Dont forget to change the CoH2 process instance into a generic GameProcess instance");
    }

    public bool IsLaunched => this.m_isLaunched;

    public ISession Session => new NullSession();

    public IMatchData GetResults() => this.m_result;

    public bool IsPerfect() => SessionUtility.HasPlayback() && !SessionUtility.GotFatalScarError() && !SessionUtility.GotBugsplat().Result;

    public void Launch() => this.m_isLaunched = this.m_process.Launch();

    public void WaitForExit() {
        if (this.m_isLaunched) {
            if (this.m_process.WatchProcess() == CoH2Process.PROCESS_OK) {
                this.m_result = new JsonPlayback(new ReplayMatchData(this.Session));
            } else {
                this.m_result = new JsonPlayback();
            }
        }
    }

}
