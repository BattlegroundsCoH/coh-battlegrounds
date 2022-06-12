using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Functional;

/// <summary>
/// Static helper class for working with generic collections.
/// </summary>
public static class FunctionalGeneric {

    /// <summary>
    /// Select a random <typeparamref name="T"/> from a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of object contained within the <see cref="IEnumerable{T}"/></typeparam>
    /// <param name="enumerable"><see cref="IEnumerable{T}"/> to select random element from.</param>
    /// <returns>A random <typeparamref name="T"/> from the <paramref name="enumerable"/>.</returns>
    public static T Random<T>(this IEnumerable<T> enumerable)
        => enumerable.Random(new Random());

    /// <summary>
    /// Select a random <typeparamref name="T"/> from a <see cref="IEnumerable{T}"/> using a <see cref="System.Random"/> object.
    /// </summary>
    /// <typeparam name="T">The type of object contained within the <see cref="IEnumerable{T}"/></typeparam>
    /// <param name="enumerable"><see cref="IEnumerable{T}"/> to select random element from.</param>
    /// <param name="random">The <see cref="System.Random"/> instance to use when selecting random <typeparamref name="T"/>.</param>
    /// <returns>A random <typeparamref name="T"/> from the <paramref name="enumerable"/>.</returns>
    public static T Random<T>(this IEnumerable<T> enumerable, Random random) {
        int i = enumerable.Any() ? random.Next(0, enumerable.Count()) : -1;
        return (i >= 0) ? enumerable.ElementAt(i) : default;
    }

    /// <summary>
    /// Iterate through each element <typeparamref name="T"/> in the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of object contained within the <see cref="IEnumerable{T}"/></typeparam>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to iterate through.</param>
    /// <param name="action">The <see cref="Action{T}"/> to invoke on each element of the <paramref name="enumerable"/>.</param>
    /// <returns>The <paramref name="enumerable"/> that was iterated through.</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext()) {
            action(enumerator.Current);
        }
        return enumerable;
    }

    /// <summary>
    /// Filters out a sequence of values
    /// </summary>
    /// <typeparam name="T">The type of object contained within the <see cref="IEnumerable{T}"/></typeparam>
    /// <param name="enumerable">The sequence to iterate through while filtering.</param>
    /// <param name="predicate">The condition to apply when filtering.</param>
    /// <returns>The <paramref name="enumerable"/> without the elements matching the <paramref name="predicate"/>.</returns>
    public static IEnumerable<T> Without<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        => enumerable.Where(x => !predicate(x));

    /// <summary>
    /// Perform a self-union on a <see cref="IEnumerable{IEnumerable{TValue}}"/>.
    /// </summary>
    /// <typeparam name="TContainer">The new containing collection type.</typeparam>
    /// <typeparam name="TValue">The inner-value type that's being contained.</typeparam>
    /// <param name="values">The enumerable of enumerables to self-union into a single <see cref="ICollection{T}"/> object.</param>
    /// <returns>A single containing <see cref="ICollection{TValue}"/> object containing all inner-values of the <paramref name="values"/> object.</returns>
    public static TContainer Union<TContainer, TValue>(this IEnumerable<IEnumerable<TValue>> values) where TContainer : ICollection<TValue> {
        TContainer container = Activator.CreateInstance<TContainer>();
        var outer = values.GetEnumerator();
        while (outer.MoveNext()) {
            var inner = outer.Current.GetEnumerator();
            while (inner.MoveNext()) {
                container.Add(inner.Current);
            }
        }
        return container;
    }

    /// <summary>
    /// Get a remove-safe iterator that iterates over <paramref name="enumerable"/>.
    /// </summary>
    /// <param name="enumerable">The enumerable to get enumerator of.</param>
    /// <returns>An eumerator that can iterate over the collection where remove functionality does not break.</returns>
    public static IEnumerator<T> GetSafeEnumerator<T>(this IEnumerable<T> enumerable) where T : class {
        List<T> dummy = new List<T>();
        lock (enumerable) {
            var itt = enumerable.GetEnumerator();
            while (itt.MoveNext()) {
                dummy.Add(itt.Current);
            }
        }
        return dummy.GetEnumerator();
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) {
        List<T> notnulls = new();
        var itt = enumerable.GetEnumerator();
        while (itt.MoveNext()) {
            if (itt.Current is not null) {
                notnulls.Add(itt.Current);
            }
        }
        return notnulls;
    }

}

