namespace Battlegrounds.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Dictionary{TKey, TValue}"/> to simplify common operations.
/// </summary>
/// <remarks>This class includes methods that enhance the functionality of dictionaries, such as retrieving or
/// computing values based on a key. These methods are designed to improve code readability and reduce boilerplate logic
/// when working with dictionaries.</remarks>
public static class DictionaryExtensions {

    /// <summary>
    /// Retrieves the value associated with the specified key from the dictionary, or computes and adds it if the key
    /// does not exist.
    /// </summary>
    /// <remarks>If the specified key does not exist in the dictionary, the <paramref name="valueFactory"/>
    /// function is invoked to compute the value. The computed value is then added to the dictionary and
    /// returned.</remarks>
    /// <typeparam name="K">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="V">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to retrieve the value from or update.</param>
    /// <param name="key">The key whose associated value is to be retrieved or computed. Cannot be null.</param>
    /// <param name="valueFactory">A function that computes the value to associate with the key if the key does not exist in the dictionary.</param>
    /// <returns>The value associated with the specified key if it exists; otherwise, the newly computed value.</returns>
    public static V GetOrCompute<K, V>(this Dictionary<K, V> dictionary, K key, Func<K, V> valueFactory) where K : notnull {
        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }
        value = valueFactory(key);
        dictionary[key] = value;
        return value;
    }

    /// <summary>
    /// Adds a key-value pair to the dictionary or updates the value for an existing key.
    /// </summary>
    /// <remarks>If the key exists in the dictionary, the <paramref name="valueFactory"/> function is used to 
    /// compute the updated value. If the key does not exist, the specified <paramref name="value"/>  is added to the
    /// dictionary.</remarks>
    /// <typeparam name="K">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="V">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to which the key-value pair will be added or updated.</param>
    /// <param name="key">The key to add or update. Must not be null.</param>
    /// <param name="value">The value to associate with the key if the key does not already exist in the dictionary.</param>
    /// <param name="valueFactory">A function that computes the new value for the key if it already exists in the dictionary. The function takes
    /// the key and the existing value as parameters and returns the updated value.</param>
    public static void AddOrUpdate<K, V>(this Dictionary<K, V> dictionary, K key, V value, Func<K, V, V> valueFactory) where K : notnull {
        if (dictionary.TryGetValue(key, out V? existing)) {
            dictionary[key] = valueFactory(key, existing);
        } else {
            dictionary[key] = value; // Update existing value
        }
    }

    /// <summary>
    /// Adds a value to the list associated with the specified key in the dictionary. If the key does not exist,
    /// initializes a new list with the value and adds it to the dictionary.
    /// </summary>
    /// <remarks>If the key already exists in the dictionary, the value is added to the existing list. If the
    /// key does not exist, a new list is created containing the value, and the key-value pair is added to the
    /// dictionary.</remarks>
    /// <typeparam name="K">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="V">The type of the values in the lists associated with the keys.</typeparam>
    /// <param name="dictionary">The dictionary to modify. Cannot be null.</param>
    /// <param name="key">The key to associate the value with. Must not be null.</param>
    /// <param name="value">The value to add to the list associated with the specified key.</param>
    public static void AddOrInitialize<K, V>(this Dictionary<K, IList<V>> dictionary, K key, V value) where K : notnull {
        if (dictionary.TryGetValue(key, out IList<V>? existing)) {
            existing.Add(value);
        } else {
            dictionary[key] = [value]; // Update existing value
        }
    }

}
