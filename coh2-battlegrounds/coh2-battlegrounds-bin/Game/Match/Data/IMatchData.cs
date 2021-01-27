using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data {
    
    /// <summary>
    /// Interface for objects containing match data.
    /// </summary>
    public interface IMatchData : IEnumerable<IMatchEvent> {

        /// <summary>
        /// Get the <see cref="ISession"/> associated with the match data.
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// Get the length of the match
        /// </summary>
        public TimeSpan Length { get; }

        /// <summary>
        /// Get if the match contains data for the given session.
        /// </summary>
        bool IsSessionMatch { get; }

        /// <summary>
        /// Get all the players in this match.
        /// </summary>
        ReadOnlyCollection<Player> Players { get; }

        /// <summary>
        /// Load the match data into memory.
        /// </summary>
        /// <param name="matchFile">The file containing the match data.</param>
        /// <returns>Will return <see langword="true"/> if the match data was loaded. Otherwise <see langword="false"/>.</returns>
        bool LoadMatchData(string matchFile);

        /// <summary>
        /// Parse the match data.
        /// </summary>
        /// <returns>Will return <see langword="true"/> if the match data was parsed without problems. Otherwise <see langword="false"/>.</returns>
        bool ParseMatchData();

    }

}
