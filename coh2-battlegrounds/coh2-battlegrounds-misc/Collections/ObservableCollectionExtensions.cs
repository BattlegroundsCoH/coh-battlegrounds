using System;
using System.Collections.ObjectModel;

namespace Battlegrounds.Misc.Collections;

/// <summary>
/// Static class containing extensions methods for <see cref="ObservableCollection{T}"/>.
/// </summary>
public static class ObservableCollectionExtensions {

    /// <summary>
    /// Clear the collection by first appending the element and then clearing the <see cref="ObservableCollection{T}"/> until only one element remains.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    /// <param name="obs">The observable collection to clear.</param>
    /// <param name="item">The item to set as only remaining element.</param>
    public static void ClearTo<T>(this ObservableCollection<T> obs, T item) {
        obs.Add(item);
        while (obs.Count > 1) {
            obs.RemoveAt(0);
        }
    }

    /// <summary>
    /// Remove all elements in the <see cref="ObservableCollection{T}"/> matching the <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    /// <param name="obs">The observable collection to remove items from.</param>
    /// <param name="predicate">The predicate to run on all items to determine if item should be removed.</param>
    public static void RemoveAll<T>(this ObservableCollection<T> obs, Predicate<T> predicate) {
        for (int i = 0; i < obs.Count; i++) {
            if (predicate(obs[i])) {
                obs.RemoveAt(i--);
            }
        }
    }

}
