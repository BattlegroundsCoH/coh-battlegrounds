using System;
using System.Collections.Generic;
using System.Linq;

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
            => dict.GetValueOrDefault<X, Y, Z>(key, default);

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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        public readonly struct MinKeyValuePair<X, Y, Z> {

            /// <summary>
            /// 
            /// </summary>
            public X Key { get; }

            /// <summary>
            /// 
            /// </summary>
            public Y Value { get; }

            /// <summary>
            /// 
            /// </summary>
            public Z Min { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="val"></param>
            /// <param name="min"></param>
            public MinKeyValuePair(X key, Y val, Z min) {
                this.Key = key;
                this.Value = val;
                this.Min = min;
            }

            public KeyValuePair<X, Y> ToKeyValue()
                => new(this.Key, this.Value);

        }

        /// <summary>
        /// Find the smallest <typeparamref name="Z"/> value in <paramref name="dict"/> using <paramref name="selector"/> on each key value pair.
        /// </summary>
        /// <typeparam name="X">The key type.</typeparam>
        /// <typeparam name="Y">The value type.</typeparam>
        /// <typeparam name="Z">The selected value type.</typeparam>
        /// <param name="dict">The dictionary to find smallest value in.</param>
        /// <param name="selector">The selector function to select a <typeparamref name="Z"/> value.</param>
        /// <returns>The smallest <typeparamref name="Z"/> from a (<typeparamref name="X"/>,<typeparamref name="Y"/>) key value pair.</returns>
        public static MinKeyValuePair<X, Y, Z> MinPair<X, Y, Z>(this IDictionary<X, Y> dict, Func<KeyValuePair<X, Y>, Z> selector) where Z : IComparable {
            MinKeyValuePair<X, Y, Z>[] pairs = dict.Select(kvp => new MinKeyValuePair<X, Y, Z>(kvp.Key, kvp.Value, selector(kvp))).ToArray();
            int i = 0;
            for (int j = 0; j < pairs.Length; j++) {
                if (pairs[j].Min.CompareTo(pairs[i].Min) < 0) {
                    i = j;
                }
            }
            return pairs[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static KeyValuePair<X, Y> MinPair<X, Y>(this IDictionary<X, Y> dict) where Y : IComparable
            => dict.MinPair(kvp => kvp.Value).ToKeyValue();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="dict"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Dictionary<X, Y> Without<X, Y>(this IDictionary<X, Y> dict, Predicate<KeyValuePair<X, Y>> predicate)
            => dict.Where(kvp => !predicate(kvp)).ToDictionary(k => k.Key, v => v.Value);

        /// <summary>
        /// Convert the <paramref name="enumerable"/> of <see cref="KeyValuePair{X, Y}"/> instances into a <see cref="Dictionary{X, Y}"/>
        /// </summary>
        /// <typeparam name="X">The key type.</typeparam>
        /// <typeparam name="Y">The value type.</typeparam>
        /// <param name="enumerable">The enumerable to generate dictionary from.</param>
        /// <returns>A <see cref="Dictionary{X, Y}"/> representation of <paramref name="enumerable"/>.</returns>
        public static Dictionary<X, Y> ToDictionary<X, Y>(this IEnumerable<KeyValuePair<X, Y>> enumerable) {
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

    }

}
