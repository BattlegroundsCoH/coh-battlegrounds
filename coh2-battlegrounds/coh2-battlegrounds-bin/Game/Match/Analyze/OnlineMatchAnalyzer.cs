using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Analyze {

    /// <summary>
    /// Multiplayer match analysis strategy for analyzing multiplayer match data. Extension of <see cref="SingleplayerMatchAnalyzer"/>.
    /// Can be extended with custom behaviour.
    /// </summary>
    public class OnlineMatchAnalyzer : SingleplayerMatchAnalyzer {

        private ReplayMatchData m_selfData;

        public override void OnPrepare(object caller, IMatchData toAnalyze) {

            // Create analysis result
            this.m_analysisResult = new EventAnalysis(toAnalyze.Session);

            // Set self data
            if (toAnalyze is ReplayMatchData replay) {
                this.m_selfData = replay;
            } else {
                // TODO: Handle
            }

        }

        public override void OnAnalyze(object caller) {
            if (!this.AnalyzePlaybackData(this.m_selfData)) {
            }
        }

    }

}
