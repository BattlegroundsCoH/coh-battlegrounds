using System;
using System.Linq;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Data.Events;

namespace Battlegrounds.Game.Match.Analyze {
    
    /// <summary>
    /// Singleplayer analysis strategy for analyzing a singleplayer match. Implementation of <see cref="IAnalyzeStrategy"/>. Can be extended with custom behaviour.
    /// </summary>
    public class SingleplayerMatchAnalyzer : IAnalyzeStrategy {

        protected IMatchData m_subject;
        protected EventAnalysis m_analysisResult;

        public IAnalyzedMatch AnalysisResult => this.m_analysisResult;

        public virtual void OnPrepare(object caller, IMatchData toAnalyze) {
            this.m_analysisResult = new EventAnalysis(toAnalyze.Session);
            this.m_subject = toAnalyze;
        }

        public virtual void OnAnalyze(object caller) {

            if (this.m_subject is ReplayMatchData replayMatchData) {
                if (!this.AnalyzeReplayData(replayMatchData)) {
                    this.m_analysisResult = null; // Invalid (Should give a null-analysis when finalizing).
                }
            } else {

                throw new NotImplementedException(); // Some other type of analysis?

            }

        }

        protected virtual bool AnalyzeReplayData(ReplayMatchData replayMatchData) {

            // Set length of match
            this.m_analysisResult.SetLength(replayMatchData.Length);

            // Set players
            this.m_analysisResult.SetPlayers(replayMatchData.Players.ToArray());

            // Register all events
            foreach (TimeEvent timeEvent in replayMatchData) {
                var reg = this.m_analysisResult.RegisterEvent(timeEvent);
                if (!reg) {
                    if (reg.WasOutsideTime) {
                        // TODO: Handle
                        return false; // For sure some problem
                    } else if (reg.ConflictingTimes) {
                        // TODO: Handle
                        return false; // Event time not adding up
                    } else {
                        // TODO: Handle
                        return false; // Duplicate event for something that doesn't allow duplicates
                    }
                }
            }

            // Return true -> Analysis "complete" for this type
            return true;

        }

        public virtual IAnalyzedMatch OnCleanup(object caller) {

            // Compile the final result
            if (!this.m_analysisResult.CompileResults()) {
                return new NullAnalysis();
            }

            // Return the analysis
            return this.m_analysisResult;

        }

    }

}
