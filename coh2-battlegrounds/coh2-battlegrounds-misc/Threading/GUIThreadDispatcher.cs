using System;
using System.Windows.Threading;

using Battlegrounds.Util.Coroutines;

namespace Battlegrounds.Misc.Threading;

// Adapter pattern...

/// <summary>
/// Class wraping a <see cref="Dispatcher"/> object for use in a coroutine context. Implements <see cref="ICoroutineDispatcher"/>.
/// </summary>
public sealed class GUIThreadDispatcher : ICoroutineDispatcher {

    private readonly Dispatcher m_dispatcher;

    /// <summary>
    /// Initialize a new <see cref="GUIThreadDispatcher"/> class with specified <paramref name="dispatcher"/>.
    /// </summary>
    /// <param name="dispatcher">The <see cref="Dispatcher"/> object to use when running coroutines.</param>
    public GUIThreadDispatcher(Dispatcher dispatcher) {
        this.m_dispatcher = dispatcher;
    }

    public void Invoke(Action action) => this.m_dispatcher?.Invoke(action);

    public static explicit operator GUIThreadDispatcher(Dispatcher dispatcher)
        => new GUIThreadDispatcher(dispatcher);

}
