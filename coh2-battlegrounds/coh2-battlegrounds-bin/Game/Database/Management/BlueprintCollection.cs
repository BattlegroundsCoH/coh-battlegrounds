using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management {

    /// <summary>
    /// A collection of <see cref="Blueprint"/> instances. Can contain inheriting types of <see cref="Blueprint"/>.
    /// </summary>
    /// <typeparam name="T">The specific <see cref="Blueprint"/> type the collection is to consist of.</typeparam>
    public class BlueprintCollection<T> : IEnumerable<T> where T : Blueprint {

        private IEnumerable<KeyValuePair<BlueprintUID, T>> m_internalDictionary; // Not meant to be an actual dictionary

        /// <summary>
        /// Get enumerable for all property bag IDs
        /// </summary>
        public IEnumerable<BlueprintUID> PropertyBagIDs => this.m_internalDictionary.Select(x => x.Key);

        /// <summary>
        /// Is this <see cref="BlueprintCollection{T}"/> a generic collection (<typeparamref name="T"/>=<see cref="Blueprint"/>).
        /// </summary>
        public bool IsGenericBlueprintCollection => typeof(T) == typeof(Blueprint);

        /// <summary>
        /// Create new <see cref="BlueprintCollection{T}"/> from an enumerable of <see cref="KeyValuePair{TKey, TValue}"/> with <typeparamref name="T"/>=TValue.
        /// </summary>
        /// <param name="initial">The initial collection of the instance</param>
        public BlueprintCollection(IEnumerable<KeyValuePair<BlueprintUID, T>> initial) {
            this.m_internalDictionary = initial;
        }

        /// <summary>
        /// Create new <see cref="BlueprintCollection{T}"/> from a <see cref="Dictionary{BlueprintUID, T}"/>.
        /// </summary>
        /// <param name="dictionaries">The initial dictionary.</param>
        public BlueprintCollection(Dictionary<BlueprintUID, Blueprint> dictionaries) {
            this.m_internalDictionary = dictionaries.Select(x => new KeyValuePair<BlueprintUID, T>(x.Key, x.Value as T));
        }

        /// <summary>
        /// Select a single (first) <typeparamref name="T"/> from the internal enumerable.
        /// </summary>
        /// <param name="predicate">The test function.</param>
        /// <returns>A single element matching the predicate.</returns>
        public T Single(Predicate<T> predicate) => this.m_internalDictionary.FirstOrDefault(x => predicate(x.Value)).Value;

        /// <summary>
        /// Get a new <see cref="BlueprintCollection{T}"/> where all <typeparamref name="T"/> instances belong to the specified mod GUID.
        /// </summary>
        /// <param name="modguid">The mod GUID to filter by.</param>
        /// <returns>A new <see cref="BlueprintCollection{T}"/> containing only <typeparamref name="T"/> instances of specifiied mod GUID.</returns>
        public BlueprintCollection<T> FilterByMod(ModGuid modguid) => new BlueprintCollection<T>(this.m_internalDictionary.Where(x => x.Value.PBGID.Mod == modguid));

        /// <summary>
        /// Get a new <see cref="BlueprintCollection{T}"/> where all <typeparamref name="T"/> instances belong to the specified mod GUID.
        /// </summary>
        /// <param name="modguid">The mod GUID to filter by.</param>
        /// <returns>A new <see cref="BlueprintCollection{T}"/> containing only <typeparamref name="T"/> instances of specifiied mod GUID.</returns>
        public BlueprintCollection<T> FilterByMod(string modguid) => new BlueprintCollection<T>(this.m_internalDictionary.Where(x => x.Value.PBGID.Mod == modguid));

        /// <summary>
        /// Get a new <see cref="BlueprintCollection{T}"/> where all <typeparamref name="T"/> instances matching the predicate.
        /// </summary>
        /// <param name="predicate">The test function.</param>
        /// <returns>A new <see cref="BlueprintCollection{T}"/> containing only <typeparamref name="T"/> instances matching the predicate.</returns>
        public BlueprintCollection<T> Filter(Predicate<T> predicate) => new BlueprintCollection<T>(this.m_internalDictionary.Where(x => predicate(x.Value)));

        public IEnumerator<T> GetEnumerator() => this.m_internalDictionary.Select(x => x.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.m_internalDictionary.Select(x => x.Value).GetEnumerator();

    }

}
