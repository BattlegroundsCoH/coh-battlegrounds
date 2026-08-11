namespace Battlegrounds.Services;

/// <summary>
/// Opens the local HTTP listener that a browser sign-in returns to.
/// </summary>
public interface ILoopbackAuthListenerFactory {

    /// <summary>
    /// Binds a listener on an ephemeral loopback port.
    /// </summary>
    ILoopbackAuthListener? TryStart();

}

/// <summary>
/// A started loopback listener waiting for the browser to be redirected back to it.
/// </summary>
public interface ILoopbackAuthListener : IDisposable {

    /// <summary>
    /// The address to hand to the API as the login session's return URL.
    /// </summary>
    string ReturnUrl { get; }

    /// <summary>
    /// Waits for the browser to arrive on <see cref="ReturnUrl"/> and reports the login session state it carries.
    /// </summary>
    Task<string?> WaitForCallbackAsync(CancellationToken cancellationToken);

}
