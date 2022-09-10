using System;

namespace Battlegrounds.ErrorHandling;

internal static class TryForget {

    public static T Try<T>(Func<T> func, T onError) {
        try {
            return func();
        } catch {
            return onError;
        }
    }

}
