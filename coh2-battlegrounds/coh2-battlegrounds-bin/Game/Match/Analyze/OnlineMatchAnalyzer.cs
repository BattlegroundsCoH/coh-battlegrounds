using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Analyze;

/// <summary>
/// Multiplayer match analysis strategy for analyzing multiplayer match data. Extension of <see cref="SingleplayerMatchAnalyzer"/>.
/// Can be extended with custom behaviour.
/// </summary>
public class OnlineMatchAnalyzer : SingleplayerMatchAnalyzer {

    private IMatchData? m_selfData;

    public override void OnPrepare(object caller, IMatchData toAnalyze) {

        // Create analysis result
        this.m_analysisResult = new EventAnalysis(toAnalyze.Session);
        this.m_selfData = toAnalyze;

    }

    public override void OnAnalyze(object caller) {
        if (!this.AnalyzePlaybackData(this.m_selfData)) {
            this.m_analysisResult = new NullAnalysis(); // Invalid (Should give a null-analysis when finalizing).
        }
    }

}
