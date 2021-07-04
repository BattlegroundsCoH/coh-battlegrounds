using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match.Analyze {
    
    /// <summary>
    /// Strategy interface for analyzing results of match data.
    /// </summary>
    public interface IAnalyzeStrategy {

        /// <summary>
        /// The result of the analysis.
        /// </summary>
        IAnalyzedMatch AnalysisResult { get; }

        /// <summary>
        /// Prepare the analysis strategy for analysis.
        /// </summary>
        /// <param name="caller">The calling instance.</param>
        /// <param name="toAnalyze">The <see cref="IMatchData"/> to analyze.</param>
        void OnPrepare(object caller, IMatchData toAnalyze);

        /// <summary>
        /// Analyze the given match data.
        /// </summary>
        /// <param name="caller">The calling instance.</param>
        void OnAnalyze(object caller);

        /// <summary>
        /// Cleanup the potential resources used by the <see cref="IAnalyzeStrategy"/>.
        /// </summary>
        /// <param name="caller">The calling instance.</param>
        /// <returns>The final <see cref="IAnalyzedMatch"/> object generated from the analysis.</returns>
        IAnalyzedMatch OnCleanup(object caller);

    }

}
