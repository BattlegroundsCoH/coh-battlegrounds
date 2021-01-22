using System.Collections.Generic;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Analyze {
    
    /// <summary>
    /// Interface for match analysis.
    /// </summary>
    public interface IAnalyzedMatch {

        /// <summary>
        /// Get if the analyzed match can be finalized.
        /// </summary>
        bool IsFinalizableMatch { get; }

        /// <summary>
        /// Get the status of units after the match.
        /// </summary>
        IReadOnlyList<UnitStatus> Units { get; }

        /// <summary>
        /// Get the players involved in the match (May include AI players).
        /// </summary>
        IReadOnlyCollection<Player> Players { get; }

        /// <summary>
        /// Get the session that was played with the match.
        /// </summary>
        ISession Session { get; }

    }

}
