using System;

namespace Battlegrounds.Util.Threading;

/// <summary>
/// Interface representing a dispatcher for dispatching an action to a segregated process.
/// </summary>
public interface IDispatcher {

    /// <summary>
    /// Invoke callback <see cref="Action"/> using designated <see cref="IDispatcher"/>.
    /// </summary>
    /// <param name="action">The callback action to invoke</param>
    void Invoke(Action action);

}
