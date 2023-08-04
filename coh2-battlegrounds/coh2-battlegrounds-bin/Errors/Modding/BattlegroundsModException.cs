using System;

using Battlegrounds.Errors.Common;
using Battlegrounds.Modding;

namespace Battlegrounds.Errors.Modding;

/// <summary>
/// Represents exceptions thrown when a fatal error has occured within a mod context.
/// </summary>
public class BattlegroundsModException : BattlegroundsException {

    /// <summary>
    /// Get the relevant mod package
    /// </summary>
    public IModPackage? Package { get; }

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsModException"/> instance with a default message.
    /// </summary>
    public BattlegroundsModException() : base("Encountered runtime exception caused by Battlegrounds mod code") {
        this.Package = null;
        this.Log();
    }

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsModException"/> instance with a default message.
    /// </summary>
    /// <param name="package">The mod package that caused this exception.</param>
    public BattlegroundsModException(IModPackage? package) : base("Encountered runtime exception caused by Battlegrounds mod code") {
        this.Package = package;
        this.Log();
    }

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsModException"/> instance with an error message.
    /// </summary>
    /// <param name="message">The message associated with the exception.</param>
    /// <param name="package">The mod package that caused this exception.</param>
    public BattlegroundsModException(string message, IModPackage? package) : base(message) {
        this.Package = package;
        this.Log();
    }

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsModException"/> instance with an error message as causing inner exception.
    /// </summary>
    /// <param name="message">The message associated with the exception.</param>
    /// <param name="package">The mod package that caused this exception.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public BattlegroundsModException(string message, IModPackage? package, Exception innerException) : base(message, innerException) {
        this.Package = package;
        this.Log();
    }

}
