using System;
using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Analyze;

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
    /// Get the status of an item after the match.
    /// </summary>
    IReadOnlyList<ItemStatus> Items { get; }

    /// <summary>
    /// Get the players involved in the match (May include AI players).
    /// </summary>
    IReadOnlyCollection<Player> Players { get; }

    /// <summary>
    /// Get the session that was played with the match.
    /// </summary>
    ISession Session { get; }

    /// <summary>
    /// Get the length of the match
    /// </summary>
    TimeSpan Length { get; }

    /// <summary>
    /// Compile the results into a finalizable form.
    /// </summary>
    /// <returns>Will return <see langword="true"/> if it was possible to compile the result. Otherwise <see langword="false"/>.</returns>
    bool CompileResults();

    /// <summary>
    /// Get if a <see cref="Player"/> is considered a winner of the <see cref="IAnalyzedMatch"/>.
    /// </summary>
    /// <param name="player">The <see cref="Player"/> instance to get winner state of.</param>
    /// <returns>Will return <see langword="true"/> if player has won. Otherwise <see langword="false"/>.</returns>
    bool IsWinner(Player player);

}

