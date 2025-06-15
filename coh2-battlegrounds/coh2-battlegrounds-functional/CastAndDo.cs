namespace Battlegrounds.Functional;

/// <summary>
/// Static functional helper functions for casting between types and performing actions if cast is possible.
/// </summary>
public static class CastAndDo {

    /// <summary>
    /// Cast <paramref name="src"/> into its <typeparamref name="V"/> representation and invoke <paramref name="action"/> on it.
    /// </summary>
    /// <typeparam name="V">The type to cast to.</typeparam>
    /// <param name="src">The source object.</param>
    /// <param name="action">The action to perform if cast was successful</param>
    public static void Cast<V>(this object? src, Action<V> action) {
        if (src is V v) {
            action(v);
        }
    }

    /// <summary>
    /// Cast <paramref name="src"/> into its <typeparamref name="V"/> representation and invoke <paramref name="action"/> on it.
    /// </summary>
    /// <typeparam name="U">The type being cast from.</typeparam>
    /// <typeparam name="V">The type being cast to.</typeparam>
    /// <param name="src">The <typeparamref name="U"/> instance to try and cast.</param>
    /// <param name="action">The action to perform if cast was successful</param>
    public static void Cast<U,V>(this U? src, Action<V> action) where V : U {
        if (src is V v) {
            action(v);
        }
    }

}
