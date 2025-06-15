using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play;

/// <summary>
/// Play strategy for players who do not have full control over the match being played by some remote <see cref="OverwatchStrategy"/>.
/// </summary>
public class MemberOverwatchStrategy : IPlayStrategy {

    private bool m_isLaunched;
    private IMatchData m_result;

    private readonly ISessionHandler sessionHandler;
    private readonly GameProcess m_process;

    /// <summary>
    /// Initialise a new <see cref="MemberOverwatchStrategy"/> instance.
    /// </summary>
    /// <param name="sessionHandler">The handler that will handle the session</param>
    public MemberOverwatchStrategy(ISessionHandler sessionHandler) {
        this.m_isLaunched = false;
        this.m_result = new NoMatchData();
        this.sessionHandler = sessionHandler;
        this.m_process = sessionHandler.GetNewGameProcess();
    }

    /// <inheritdoc/>
    public bool IsLaunched => this.m_isLaunched;

    /// <inheritdoc/>
    public ISession Session => new NullSession();

    /// <inheritdoc/>
    public IMatchData GetResults() => this.m_result;

    /// <inheritdoc/>
    public bool IsPerfect() => sessionHandler.HasPlayback() && !sessionHandler.GotFatalScarError() && !sessionHandler.GotBugsplat().Result;

    /// <inheritdoc/>
    public void Launch() => this.m_isLaunched = this.m_process.Launch();

    /// <inheritdoc/>
    public void WaitForExit() {
        if (this.m_isLaunched) {
            if (this.m_process.WatchProcess() == GameProcess.PROCESS_OK) {
                this.m_result = new JsonPlayback(new ReplayMatchData(this.Session));
            } else {
                this.m_result = new JsonPlayback();
            }
        }
    }

}
