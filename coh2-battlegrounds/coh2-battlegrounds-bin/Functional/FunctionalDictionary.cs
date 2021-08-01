using System.Collections.Generic;

namespace Battlegrounds.Functional {

    /// <summary>
    /// Static extension class for <see cref="IDictionary{TKey, TValue}"/> instances.
    /// </summary>
    public static class FunctionalDictionaryExtensions {

        /// <summary>
        /// Get value of element tied to <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="U">The key type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
        /// <typeparam name="V">The value type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
        /// <param name="dict">The dictionary instance to retrieve default value from.</param>
        /// <param name="key">The key of the element to try and get value of.</param>
        /// <returns>If value is not found, <see langword="default"/> value of <typeparamref name="V"/>; Otherwise the found value.</returns>
        public static V GetValueOrDefault<U, V>(this IDictionary<U, V> dict, U key)
            => dict.GetValueOrDefault(key, default);

        /// <summary>
        /// Get value of element tied to <paramref name="key"/> or <paramref name="defaultValue"/> if not found in dictionary.
        /// </summary>
        /// <typeparam name="U">The key type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
        /// <typeparam name="V">The value type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
        /// <param name="dict">The dictionary instance to retrieve default value from.</param>
        /// <param name="key">The key of the element to try and get value of.</param>
        /// <param name="defaultValue">The default value to return if element is not found.</param>
        /// <returns>If value is not found, <paramref name="defaultValue"/>; Otherwise the found value.</returns>
        public static V GetValueOrDefault<U, V>(this IDictionary<U, V> dict, U key, V defaultValue)
            => dict.ContainsKey(key) ? dict[key] : defaultValue;

        /// <summary>
        /// Get value of element tied to <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="X">The key type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
        /// <typeparam name="Y">The value type of the <see cref="IDictionary{TKey, TValue}"/></typeparam>
        /// <typeparam name="Z">The expected value type.</typeparam>
        /// <param name="dict">The dictionary instance to retrieve default value from.</param>
        /// <param name="key">The key of the element to try and get value of.</param>
        /// <returns>If value is not found, <see langword="default"/> value of <typeparamref name="Z"/>; Otherwise the found value.</returns>
        public static Z GetValueOrDefault<X, Y, Z>(this IDictionary<X, Y> dict, X key) where Z : Y
            => dict.GetValueOrDefault<X,Y,Z>(key, default);

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
        public static Z GetValueOrDefault<X, Y, Z>(this IDictionary<X, Y> dict, X key, Z defaultValue) where Z : Y
            => dict.ContainsKey(key) ? (Z)dict[key] : defaultValue;

    }

}
