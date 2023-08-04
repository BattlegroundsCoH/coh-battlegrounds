namespace Battlegrounds.Errors.Common;

/// <summary>
/// Represents an error related to redo actions.
/// </summary>
public class RedoActionFailedException : BattlegroundsException {

    /// <summary>
    /// Initialise a new <see cref="RedoActionFailedException"/> with a default message.
    /// </summary>
    public RedoActionFailedException() { }

    /// <summary>
    /// Initialise a new <see cref="RedoActionFailedException"/> with a message.
    /// </summary>
    /// <param name="message">Message associated with exception.</param>
    public RedoActionFailedException(string message) : base(message) { }

}
