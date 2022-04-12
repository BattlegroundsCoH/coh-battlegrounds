using System;
using System.Diagnostics;

namespace Battlegrounds.ErrorHandling;

/// <summary>
/// 
/// </summary>
public class FatalAppException : Exception {

    private const string DefaultMessage = "Fatal application error.";

    /// <summary>
    /// 
    /// </summary>
    public FatalAppException() : base(DefaultMessage) => this.Crash(DefaultMessage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public FatalAppException(string message) : base(message) => this.Crash(message);

    private void Crash(string message) {
        Trace.WriteLine($"Fatal app error detected and application will now close ({message}).", nameof(FatalAppException));
        Environment.Exit(0);
    }

}
