using System;

namespace Battlegrounds.Util.Coroutines;

public sealed class DefaultCoroutineDispatcher : ICoroutineDispatcher {

    public void Invoke(Action action) => action?.Invoke();

}
