using System.Diagnostics;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Networking.Remoting;

namespace Battlegrounds.ErrorHandling.Networking;

/// <summary>
/// Class representing errors that occured while trying to invoke a remote call procedure.
/// </summary>
public class RemoteCallException : BattlegroundsException {

    /// <summary>
    /// Get the name of the remote method that was invoked.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Get the arguments given to the remote method that causd the exception.
    /// </summary>
    public object[] Arguments { get; }

    /// <summary>
    /// Initialise a new <see cref="RemoteCallException"/> instance with exception data.
    /// </summary>
    /// <param name="method">The name of the method that was invoked remotely.</param>
    /// <param name="args">The arguments given to the remote call.</param>
    /// <param name="errorMessage">The error message returned by the remote destination.</param>
    public RemoteCallException(string method, object[] args, string errorMessage) : base(errorMessage) {
        this.Method = method;
        this.Arguments = args;
        Trace.WriteLine($"Error in call '{method}({string.Join(',', args)})':\n\t{errorMessage}", nameof(RemoteCall));
    }

}
