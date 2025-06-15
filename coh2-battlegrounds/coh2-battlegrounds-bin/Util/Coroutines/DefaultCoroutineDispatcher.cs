using System;

namespace Battlegrounds.Util.Coroutines;

/// <summary>
/// Class representing a basic coroutine dispatcher that immediately invokes the given input.
/// </summary>
public sealed class DefaultCoroutineDispatcher : ICoroutineDispatcher {

    /// <inheritdoc/>
    public void Invoke(Action action) => action?.Invoke();

}
