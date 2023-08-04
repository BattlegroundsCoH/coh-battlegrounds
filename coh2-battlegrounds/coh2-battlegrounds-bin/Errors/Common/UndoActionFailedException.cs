namespace Battlegrounds.Errors.Common;

/// <summary>
/// Represents an error related to undo actions.
/// </summary>
public class UndoActionFailedException : BattlegroundsException {

    /// <summary>
    /// Initialise a new <see cref="UndoActionFailedException"/> instance.
    /// </summary>
    public UndoActionFailedException() : base() { }

    /// <summary>
    /// Initialise a new <see cref="UndoActionFailedException"/> instance with a message.
    /// </summary>
    /// <param name="message">Message associated with exception.</param>
    public UndoActionFailedException(string message) : base(message) { }

}
