using Battlegrounds.Util.Threading;

namespace Battlegrounds.Util.Coroutines;

/// <summary>
/// Interface for coroutine dispatching handler.
/// </summary>
public interface ICoroutineDispatcher : IDispatcher {

    /// <summary>
    /// Get the default dispatcher (Runs coroutine on background dispatcher)
    /// </summary>
    public static ICoroutineDispatcher CurrentDispatcher
        => new DefaultCoroutineDispatcher();

}
