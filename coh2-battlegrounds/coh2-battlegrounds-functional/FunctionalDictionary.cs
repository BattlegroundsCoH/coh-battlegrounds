using System;
using System.Collections.Generic;

namespace Battlegrounds.Functional; 

/// <summary>
/// Static extension class for <see cref="IDictionary{TKey, TValue}"/> instances.
/// </summary>
public static class FunctionalDictionaryExtensions {

    /// <summary>
    /// Get value of element tied to <paramref name="key"/> or <paramref name="defaultValue"/> if not found in dictionary.
    /// </summary>
    /// <typeparam name="U">The key type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
    /// <typeparam name="V">The value type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
    /// <param name="dict">The dictionary instance to retrieve default value from.</param>
    /// <param name="key">The key of the element to try and get value of.</param>
    /// <param name="defaultValue">The default value to return if element is not found.</param>
    /// <returns>If value is not found, <paramref name="defaultValue"/>; Otherwise the found value.</returns>
    public static V GetOrDefault<U, V>(this IDictionary<U, V> dict, U key, V defaultValue)
        => dict.ContainsKey(key) ? dict[key] : defaultValue;

    /// <summary>
    /// Get value of element tied to <paramref name="key"/> or <paramref name="defaultValue"/> if not found in dictionary.
    /// </summary>
    /// <typeparam name="X">The key type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
    /// <typeparam name="Y">The value type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
    /// <typeparam name="Z">The expected value type.</typeparam>
    /// <param name="dict">The dictionary instance to retrieve default value from.</param>
    /// <param name="key">The key of the element to try and get value of.</param>
    /// <param name="defaultValue">The default value to return if element is not found.</param>
    /// <returns>If value is not found, <paramref name="defaultValue"/>; Otherwise the found value.</returns>
    public static Z GetCastValueOrDefault<X, Y, Z>(this IDictionary<X, Y> dict, X key, Z defaultValue) 
        where Z : Y 
        where X : notnull
        where Y : notnull
        => dict.ContainsKey(key) ? (Z)dict[key] : defaultValue;

    /// <summary>
    /// Convert the <paramref name="enumerable"/> of <see cref="KeyValuePair{X, Y}"/> instances into a <see cref="Dictionary{X, Y}"/>
    /// </summary>
    /// <typeparam name="X">The key type.</typeparam>
    /// <typeparam name="Y">The value type.</typeparam>
    /// <param name="enumerable">The enumerable to generate dictionary from.</param>
    /// <returns>A <see cref="Dictionary{X, Y}"/> representation of <paramref name="enumerable"/>.</returns>
    public static Dictionary<X, Y> ToDictionary<X, Y>(this IEnumerable<KeyValuePair<X, Y>> enumerable) where X : notnull {
        Dictionary<X, Y> obj = new();
        IEnumerator<KeyValuePair<X, Y>> itt = enumerable.GetEnumerator();
        while (itt.MoveNext()) {
            if (obj.ContainsKey(itt.Current.Key)) {
                throw new ArgumentException($"Duplicate key entry '{itt.Current.Key}'");
            }
            obj.Add(itt.Current.Key, itt.Current.Value);
        }
        return obj;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static T[] Map<K,V,T>(this IDictionary<K,V> dic, Func<K,V,T> func) {
        var mapped = new T[dic.Count];
        var itt = dic.GetEnumerator();
        int i = 0;
        while (itt.MoveNext()) {
            mapped[i++] = func(itt.Current.Key, itt.Current.Value);
        }
        return mapped;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="dic"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static bool Exists<K,V>(this IDictionary<K,V> dic, Predicate<KeyValuePair<K,V>> predicate) {
        var itt = dic.GetEnumerator();
        while (itt.MoveNext()) {
            if (predicate(itt.Current)) {
                return true;
            }
        }
        return false;
    }

}
