using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play {

    /// <summary>
    /// <see cref="IPlayStrategy"/> implementation for watching over the game process while it runs.
    /// </summary>
    public sealed class OverwatchStrategy : IPlayStrategy {

        private readonly ISession m_session;

        private bool m_hasLaunched;
        private int m_procResponse;
        private IMatchData? m_matchData;

        public ISession Session => this.m_session;

        /// <summary>
        /// Create new <see cref="OverwatchStrategy"/> for the specified <see cref="ISession"/>.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> instance used to run the game.</param>
        public OverwatchStrategy(ISession session) {
            this.m_hasLaunched = false;
            this.m_matchData = null;
            this.m_procResponse = -1;
            this.m_session = session;
        }

        public bool IsLaunched => this.m_hasLaunched;

        public bool IsPerfect()
            => this.m_procResponse == CoH2Launcher.PROCESS_OK &&
            !SessionUtility.GotBugsplat().Result &&
            !SessionUtility.GotFatalScarError() &&
            SessionUtility.HasPlayback();

        public void Launch() {
            if (!this.IsLaunched) {
                this.m_hasLaunched = CoH2Launcher.Launch();
            }
        }

        public void WaitForExit() {
            if (this.IsLaunched) {
                this.m_procResponse = CoH2Launcher.WatchProcess();
            }
        }

        public IMatchData GetResults() {
            if (this.m_matchData is null) {
                this.m_matchData = new ReplayMatchData(this.m_session);
            }
            return this.m_matchData;
        }

    }

}
