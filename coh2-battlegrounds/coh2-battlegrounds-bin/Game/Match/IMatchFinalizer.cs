using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Finalizer;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Interface for an object capable of finalizing a Company of Heroes 2 Battlegrounds match, such that changes that occured during the match are applied where relevant.
    /// </summary>
    public interface IMatchFinalizer {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="analyzedMatch"></param>
        void Finalize(IFinalizeStrategy strategy, IAnalyzedMatch analyzedMatch);

    }

}
