using Battlegrounds.ErrorHandling.CommonExceptions;

namespace Battlegrounds.ErrorHandling.Networking {

    /// <summary>
    /// Exception class thrown when a connection failed.
    /// </summary>
    public class ConnectionFailedException : BattlegroundsException {

        /// <summary>
        /// Initialise a new base <see cref="ConnectionFailedException"/> instance with a specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The failure message to display.</param>
        public ConnectionFailedException(string message) : base(message) { }

    }

}
