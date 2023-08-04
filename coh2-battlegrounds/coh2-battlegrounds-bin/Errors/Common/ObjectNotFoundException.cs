namespace Battlegrounds.Errors.Common;

/// <summary>
/// Exception thrown when a requested object is not found.
/// </summary>
public class ObjectNotFoundException : BattlegroundsException {
    
    /// <summary>
    /// Initialise a new <see cref="ObjectNotFoundException"/> instance with no descriptors.
    /// </summary>
    public ObjectNotFoundException() : base("Requested object not found") { }
    
    /// <summary>
    /// Initialise a new <see cref="ObjectNotFoundException"/> instance with a custom <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The information message to show.</param>
    public ObjectNotFoundException(string message) : base(message) { }

}
