using System;

using Battlegrounds.Errors.Common;

namespace Battlegrounds.Errors.IO;

/// <summary>
/// Represents errors where an invalid file contents (binary error, structural errror etc.) was detected.
/// </summary>
public class InvalidFileContentsException : BattlegroundsException {

    /// <summary>
    /// Initialise a new <see cref="InvalidFileContentsException"/> instance.
    /// </summary>
    public InvalidFileContentsException() : base("Invalid file contents.") {
    }

    /// <summary>
    /// Initialise a new <see cref="InvalidFileContentsException"/> instance with an exception message.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    public InvalidFileContentsException(string message) : base(message) {
    }

    /// <summary>
    /// Initialise a new <see cref="InvalidFileContentsException"/> instance enclosing an inner <see cref="Exception"/> with an exception message.
    /// </summary>
    /// <param name="message">The message describing the exception.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public InvalidFileContentsException(string message, Exception innerException) : base(message, innerException) {
    }

}
