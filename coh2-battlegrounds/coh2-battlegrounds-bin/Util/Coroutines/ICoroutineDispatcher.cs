using System;

namespace Battlegrounds.Util.Coroutines;

/// <summary>
/// Interface for coroutine dispatching handler.
/// </summary>
public interface ICoroutineDispatcher {

    /// <summary>
    /// Get the default dispatcher (Runs coroutine on background dispatcher)
    /// </summary>
    public static ICoroutineDispatcher CurrentDispatcher
        => new DefaultCoroutineDispatcher();

    /// <summary>
    /// Invoke callback <see cref="Action"/> using designated <see cref="ICoroutineDispatcher"/>.
    /// </summary>
    /// <param name="action">The callback action to invoke</param>
    void Invoke(Action action);

}

