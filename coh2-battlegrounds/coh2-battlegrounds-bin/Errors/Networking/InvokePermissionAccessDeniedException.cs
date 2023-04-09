using Battlegrounds.Errors.Common;

namespace Battlegrounds.Errors.Networking;

/// <summary>
/// Exception class thrown when an invoke permission is denied due to permission level
/// </summary>
public class InvokePermissionAccessDeniedException : BattlegroundsException {

    /// <summary>
    /// Get the reason for permission denied.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Initialise a new <see cref="InvokePermissionAccessDeniedException"/> instance with a specified <paramref name="reason"/>.
    /// </summary>
    /// <param name="reason">The reason for the exception.</param>
    public InvokePermissionAccessDeniedException(string reason) : base($"Invoke permission deniced: {reason}")
        => this.Reason = reason;

}
