using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cause"></param>
    /// <param name="message"></param>
    public delegate void AnalysisCancelledHandler(object cause, string message);

    /// <summary>
    /// Interface for an object capable of analyzing the result of a Company of Heroes 2 Battlegrounds match.
    /// </summary>
    public interface IMatchAnalyzer {

        /// <summary>
        /// The result of the analysis performed by the <see cref="IMatchAnalyzer"/>.
        /// </summary>
        IAnalyzedMatch MatchAnalysis { get; }

        /// <summary>
        /// Occurs when the analysis is cancelled
        /// </summary>
        event AnalysisCancelledHandler AnalysisCancelled;

        /// <summary>
        /// Analyze the most recent Company of Heroes 2 Battlegrounds. This will apply relevant changes to calling <see cref="Session"/>.
        /// </summary>
        /// <param name="strategy">The strategy to use when analyzing the match data.</param>
        /// <param name="matchResults">The match data to analyze.</param>
        void Analyze(IAnalyzeStrategy strategy, IMatchData matchResults);

    }

}
