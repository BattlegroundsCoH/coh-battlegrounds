using System;
using System.Diagnostics;

using Battlegrounds.Errors.Common;

namespace Battlegrounds.Errors.Networking;

/// <summary>
/// Represents exceptions thrown when errors occur while trying to marshal or unmarshal Go data.
/// </summary>
public class GoMarshalException : BattlegroundsException {

    /// <summary>
    /// Initialise a new <see cref="GoMarshalException"/> instance with an error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public GoMarshalException(string message) : base(message) { }

    /// <summary>
    /// Initialise a new <see cref="GoMarshalException"/> instance with an error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception causing this exception.</param>
    public GoMarshalException(string message, Exception innerException) : base(message, innerException) {
        Trace.WriteLine(innerException, nameof(GoMarshalException));
    }

}
