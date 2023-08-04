using Battlegrounds.Errors.Common;

namespace Battlegrounds.Errors.Networking;

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
