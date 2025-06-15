using System;
using System.Diagnostics;

namespace Battlegrounds.Errors.Common;

/// <summary>
/// Represents the base exception class for exceptions directly related to code within the Battlegrounds environment.
/// </summary>
public class BattlegroundsException : Exception {

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsException"/> instance with a default message.
    /// </summary>
    public BattlegroundsException() : base("Encountered runtime exception caused by Battlegrounds code") {
        this.Log();
    }

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsException"/> instance with an error message.
    /// </summary>
    /// <param name="message">The message associated with the exception.</param>
    public BattlegroundsException(string message) : base(message) {
        this.Log();
    }

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsException"/> instance with an error message as causing inner exception.
    /// </summary>
    /// <param name="message">The message associated with the exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public BattlegroundsException(string message, Exception innerException) : base(message, innerException) {
        this.Log();
    }

    /// <summary>
    /// Log the battlegrounds exception.
    /// </summary>
    public void Log() => Trace.WriteLine(this, nameof(BattlegroundsException));

}
