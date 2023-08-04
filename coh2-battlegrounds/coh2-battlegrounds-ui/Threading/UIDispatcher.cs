using System;
using System.Windows.Threading;

using Battlegrounds.Util.Threading;

namespace Battlegrounds.UI.Threading;

/// <summary>
/// Class representing a dispatcher on the UI thread.
/// </summary>
public sealed class UIDispatcher : IDispatcher {

    private readonly Dispatcher dispatcher;

    /// <summary>
    /// Initialise a new <see cref="UIDispatcher"/> instance.
    /// </summary>
    /// <param name="dispatcher">The <see cref="Dispatcher"/> to dispatch through.</param>
    public UIDispatcher(Dispatcher dispatcher) { 
        this.dispatcher = dispatcher;
    }

    /// <summary>
    /// Executes the specified <paramref name="action"/> through the given dispatcher.
    /// </summary>
    /// <param name="action">The action to invoke on the UI thread.</param>
    public void Invoke(Action action) => dispatcher.Invoke(action);

}
