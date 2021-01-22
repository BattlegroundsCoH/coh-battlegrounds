using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play {

    /// <summary>
    /// Play strategy for players who do not have full control over the match being played by some remote <see cref="OverwatchStrategy"/>.
    /// </summary>
    public class MemberOverwatchStrategy : IPlayStrategy {

        private bool m_isLaunched;
        private ISession m_session;
        private IMatchData m_result;

        public MemberOverwatchStrategy(ISession session) {
            this.m_isLaunched = false;
            this.m_session = session;
        }

        public bool IsLaunched => this.m_isLaunched;

        public ISession Session => this.m_session;

        public IMatchData GetResults() => this.m_result;

        public bool IsPerfect() => SessionUtility.HasPlayback() && !SessionUtility.GotFatalScarError() && !SessionUtility.GotBugsplat().Result;

        public void Launch() => this.m_isLaunched = CoH2Launcher.Launch();

        public void WaitForExit() { 
            if (this.m_isLaunched) {
                if (CoH2Launcher.WatchProcess() == CoH2Launcher.PROCESS_OK) {
                    this.m_result = new JsonPlayback(new ReplayMatchData(this.m_session));
                } else {
                    this.m_result = new JsonPlayback();
                }
            }
        }

    }

}
