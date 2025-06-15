using Battlegrounds.Errors.Common;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// Exception that is thrown when a blueprint is of an invalid type.
/// </summary>
public class InvalidBlueprintTypeException : BattlegroundsException {

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBlueprintTypeException"/> class
    /// with no message.
    /// </summary>
    public InvalidBlueprintTypeException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBlueprintTypeException"/> class
    /// with the specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidBlueprintTypeException(string message) : base(message) { }

}
