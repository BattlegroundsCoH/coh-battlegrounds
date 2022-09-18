using System;
using System.Diagnostics;

namespace Battlegrounds.Game.Match.Data.Events;

/// <summary>
/// The type of verification to apply
/// </summary>
public enum VerificationType {

    /// <summary>
    /// Verify session ID
    /// </summary>
    SessionVerification,

    // TODO: Add more verification (if needed)

}

/// <summary>
/// <see cref="IMatchEvent"/> implementation for handling verification.
/// </summary>
public sealed class VerificationEvent : IMatchEvent {

    public char Identifier => 'G';
    public uint Uid { get; }

    /// <summary>
    /// Get the type of verification to do.
    /// </summary>
    public VerificationType VerificationType { get; }

    /// <summary>
    /// Get the argument that's used for verification.
    /// </summary>
    public string VerificationArgument { get; }

    /// <summary>
    /// Create a new <see cref="VerificationEvent"/> with verification data.
    /// </summary>
    /// <param name="values">The verification data to apply</param>
    public VerificationEvent(uint id, string[] values) {
        this.Uid = id;
        if (values.Length == 1) {
            this.VerificationType = VerificationType.SessionVerification;
            this.VerificationArgument = values[0];
        } else {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Verify that the event is as expected.
    /// </summary>
    /// <param name="argument">The argument to use when verifying.</param>
    /// <returns>Returns <see langword="true"/> if verification succeeds. Otherwise <see langword="false"/></returns>
    public bool Verify(object argument) {
        if (this.VerificationType == VerificationType.SessionVerification && argument is Session session) {
            Trace.WriteLine($"{this.VerificationArgument} ?= {session.SessionID}", "VerificationEvent");
            return this.VerificationArgument.CompareTo(session.SessionID.ToString()) == 0;
        }
        return false;
    }

}
