using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play;

/// <summary>
/// <see cref="IPlayStrategy"/> implementation for watching over the game process while it runs.
/// </summary>
public sealed class OverwatchStrategy : IPlayStrategy {

    private readonly ISession m_session;
    private readonly ISessionHandler sessionHandler;
    private readonly GameProcess m_gameProcess;

    private bool m_hasLaunched;
    private int m_procResponse;
    private IMatchData? m_matchData;

    /// <inheritdoc/>
    public ISession Session => this.m_session;

    /// <inheritdoc/>
    public bool IsLaunched => this.m_hasLaunched;

    /// <summary>
    /// Create new <see cref="OverwatchStrategy"/> for the specified <see cref="ISession"/>.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/> instance used to run the game.</param>
    /// <param name="sessionHandler">The session handler to use</param>
    public OverwatchStrategy(ISession session, ISessionHandler sessionHandler) {
        this.m_hasLaunched = false;
        this.m_matchData = null;
        this.m_procResponse = -1;
        this.m_session = session;
        this.sessionHandler = sessionHandler;
        this.m_gameProcess = sessionHandler.GetNewGameProcess();
    }

    /// <inheritdoc/>
    public bool IsPerfect()
        => this.m_procResponse == GameProcess.PROCESS_OK &&
        !sessionHandler.GotBugsplat().Result &&
        !sessionHandler.GotFatalScarError() &&
        sessionHandler.HasPlayback();

    /// <inheritdoc/>
    public void Launch() {
        if (!this.IsLaunched) {
            this.m_hasLaunched = this.m_gameProcess.Launch();
        }
    }

    /// <inheritdoc/>
    public void WaitForExit() {
        if (this.IsLaunched) {
            this.m_procResponse = this.m_gameProcess.WatchProcess();
        }
    }

    /// <inheritdoc/>
    public IMatchData GetResults() {
        this.m_matchData ??= new ReplayMatchData(this.m_session);
        return this.m_matchData;
    }

}
