using System;
using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Play {

    public class BattleSimulatorStrategy : IPlayStrategy {

        private bool m_simScarError;
        private bool m_isLaunched;
        private SimulatedMatchData m_matchData;

        public bool IsLaunched => this.m_isLaunched;

        public ISession Session { get; }

        public IMatchData GetResults() => this.m_matchData;

        public BattleSimulatorStrategy(ISession session) {
            this.Session = session;
            this.m_matchData = new SimulatedMatchData(this.Session);
        }

        public bool IsPerfect() => !this.m_simScarError;

        public void Launch() => this.m_isLaunched = true;

        public void ScarError() => this.m_simScarError = true;

        public void BattleEvent(TimeSpan timeStamp, IMatchEvent matchEvent) => this.m_matchData.AddEvent(timeStamp, matchEvent);

        public void WaitForExit() { }

    }

}
