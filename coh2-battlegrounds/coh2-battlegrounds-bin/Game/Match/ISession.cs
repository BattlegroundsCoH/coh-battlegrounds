using System;
using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Game.Match {
    
    /// <summary>
    /// Interface for session data for a session to be played.
    /// </summary>
    public interface ISession {
        
        /// <summary>
        /// Get the ID given to the session.
        /// </summary>
        Guid SessionID { get; }

        /// <summary>
        /// Get a value determining if the <see cref="ISession"/> allow for persistency (events ingame will be saved in the company).
        /// </summary>
        bool AllowPersistency { get; }

        /// <summary>
        /// Get the <see cref="Company"/> for the player with the given <paramref name="steamID"/>.
        /// </summary>
        /// <param name="steamID">The unique steam ID of the player to get the <see cref="Company"/> of.</param>
        /// <returns>The <see cref="Company"/> belonging to the player with given <paramref name="steamID"/>. Otherwise <see langword="null"/>.</returns>
        Company GetPlayerCompany(ulong steamID);

    }

}
