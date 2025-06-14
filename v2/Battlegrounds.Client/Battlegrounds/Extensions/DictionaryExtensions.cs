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

}
