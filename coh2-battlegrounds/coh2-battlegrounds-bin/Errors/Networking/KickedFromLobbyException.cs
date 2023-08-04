using Battlegrounds.Errors.Common;

namespace Battlegrounds.Errors.Networking;

/// <summary>
/// Exception class thrown when the local machine is kicked from the lobby.
/// </summary>
public class KickedFromLobbyException : BattlegroundsException {

    /// <summary>
    /// Initialsie a new <see cref="KickedFromLobbyException"/> instance with a specified <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The message to display when kicked.</param>
    public KickedFromLobbyException(string message) : base(message) { }

}
