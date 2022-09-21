using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Functional;

/// <summary>
/// Functional helper functions for arrays.
/// </summary>
public static class FunctionalArray {

    /// <summary>
    /// Loop through each element in an array and execute a function that cannot mutate the element.
    /// </summary>
    /// <typeparam name="T">The type the array is of.</typeparam>
    /// <param name="array">The array to run method on.</param>
    /// <param name="act">The function to run on each element.</param>
    /// <returns>Returns the given array.</returns>
    public static T[] ForEach<T>(this T[] array, Action<T> act) {
        for (int i = 0; i < array.Length; i++) {
            act(array[i]);
        }
        return array;
    }

    /// <summary>
    /// Filter all elements in <paramref name="array"/> such that only elements matching the <paramref name="filter"/> predicate 
    /// are returned in a new array.
    /// </summary>
    /// <typeparam name="T">The type the elements in the array are of.</typeparam>
    /// <param name="array">The array to filter elements from.</param>
    /// <param name="filter">The filter predicate method.</param>
    /// <returns>A new array of elements from <paramref name="array"/> matching the <paramref name="filter"/> predicate.</returns>
    public static T[] Filter<T>(this T[] array, Predicate<T> filter) {
        List<T> list = new(array.Length);
        for (int i = 0; i < array.Length; i++) {
            if (filter(array[i])) {
                list.Add(array[i]);
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// Filters all elements in <paramref name="array"/> such that when an element in array is mapped using <paramref name="map"/> the resulting value is <paramref name="equal"/>.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TMapped"></typeparam>
    /// <param name="array">The array to filter elements from.</param>
    /// <param name="map">The mapping function.</param>
    /// <param name="equal">The element to check equality against.</param>
    /// <returns>The filtered list of elements.</returns>
    public static TSource[] Filter<TSource, TMapped>(this TSource[] array, Func<TSource, TMapped> map, TMapped equal) where TMapped : notnull {
        TSource[] result = new TSource[array.Length];
        int k = 0;
        for (int i = 0; i < array.Length; i++) {
            var e = map(array[i]);
            if (e.Equals(equal)) {
                result[k++] = array[i];
            }
        }
        return result[..k];
    }

    /// <summary>
    /// Maps an array of type <typeparamref name="U"/> into an array of type <typeparamref name="V"/> through a mapping function.
    /// </summary>
    /// <typeparam name="U">The original type of the array.</typeparam>
    /// <typeparam name="V">The new type of the array.</typeparam>
    /// <param name="array">The array to map over.</param>
    /// <param name="func">The map funcion to map a single element from <typeparamref name="U"/> to <typeparamref name="V"/>.</param>
    /// <returns>An array consisting of elements of type <typeparamref name="V"/>.</returns>
    public static V[] Map<U, V>(this U[] array, Func<U, V> func) {
        V[] result = new V[array.Length];
        for (int i = 0; i < array.Length; i++) {
            result[i] = func(array[i]);
        }
        return result;
    }

    /// <summary>
    /// Maps an array of type <typeparamref name="U"/> into an array of type <typeparamref name="V"/> through a mapping function.
    /// </summary>
    /// <typeparam name="U">The original type of the array.</typeparam>
    /// <typeparam name="V">The new type of the array.</typeparam>
    /// <param name="array">The array to map over.</param>
    /// <param name="func">The map funcion to map a single element from <typeparamref name="U"/> to <typeparamref name="V"/>.</param>
    /// <returns>An array consisting of elements of type <typeparamref name="V"/>.</returns>
    public static V[] Map<U, V>(this IList<U> array, Func<U, V> func) {
        V[] result = new V[array.Count];
        for (int i = 0; i < array.Count; i++) {
            result[i] = func(array[i]);
        }
        return result;
    }

    /// <summary>
    /// Maps an array of type <typeparamref name="U"/> into an array of type <typeparamref name="V"/> through a mapping function where the index of the element is given.
    /// </summary>
    /// <typeparam name="U">The original type of the array.</typeparam>
    /// <typeparam name="V">The new type of the array.</typeparam>
    /// <param name="array">The array to map over.</param>
    /// <param name="func">The map funcion to map a single element from <typeparamref name="U"/> to <typeparamref name="V"/>. With the index.</param>
    /// <returns>An array consisting of elements of type <typeparamref name="V"/>.</returns>
    public static V[] Mapi<U, V>(this U[] array, Func<int, U, V> func) {
        V[] result = new V[array.Length];
        for (int i = 0; i < array.Length; i++) {
            result[i] = func(i, array[i]);
        }
        return result;
    }

    /// <summary>
    /// Maps an array of type <typeparamref name="U"/> into an array of type <typeparamref name="V"/> through a mapping function.
    /// </summary>
    /// <remarks>
    /// Enforces all mapped elements are not <see langword="null"/>.
    /// </remarks>
    /// <typeparam name="U">The original type of the array.</typeparam>
    /// <typeparam name="V">The new type of the array.</typeparam>
    /// <param name="array">The array to map over.</param>
    /// <param name="func">The map funcion to map a single element from <typeparamref name="U"/> to <typeparamref name="V"/>.</param>
    /// <returns>An array consisting of elements of type <typeparamref name="V"/>.</returns>
    public static V[] MapNotNull<U, V>(this U[] array, Func<U, V?> func) where V : notnull
        => array.Map(func).NotNull();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static V[] MapType<U,V>(this U[] array) where V : U where U : notnull {
        V[] result = new V[array.Length];
        for (int i = 0; i < array.Length; i++) {
            result[i] = (V)array[i];
        }
        return result;
    }

    /// <summary>
    /// Takes all values in input array and returns a new array consisting of all non-null values from input.
    /// </summary>
    /// <typeparam name="V">The element type in array-</typeparam>
    /// <param name="array">The array to pick non-null values from.</param>
    /// <returns>A new array consisting of only non-null values</returns>
    public static V[] NotNull<V>(this V?[] array) {
        V[] res = new V[array.Length];
        int j = 0;
        for (int i = 0; i < res.Length; i++) { 
            if (array[i] is V v) {
                res[j++] = v;
            }
        }
        return res[..j];
    }

    /// <summary>
    /// Flattens a jagged array into one continous array. Elements will preserve the order of the arrays.
    /// </summary>
    /// <typeparam name="T">The type the elements in the array are of.</typeparam>
    /// <param name="array">The jagged array to flatten.</param>
    /// <returns>A new array of a single dimension.</returns>
    public static T[] Flatten<T>(this T[][] array) {
        List<T> ts= new List<T>();
        for (int i = 0; i < array.Length; i++) {
            ts.AddRange(array[i]);
        }
        return ts.ToArray();
    }

    /// <summary>
    /// Convert an array of type <typeparamref name="U"/> into an array of type <typeparamref name="V"/> through a converter 
    /// taking <typeparamref name="U"/> as input and returning an array of type <typeparamref name="V"/>.
    /// </summary>
    /// <typeparam name="U">The original type of the array.</typeparam>
    /// <typeparam name="V">The new type of the array.</typeparam>
    /// <param name="array">The <typeparamref name="U"/> array to convert.</param>
    /// <param name="func">The conversion method to convert a single element from <typeparamref name="U"/> to an array of type <typeparamref name="V"/>.</param>
    /// <returns>An array consisting of elements of type <typeparamref name="V"/>.</returns>
    public static V[] MapAndFlatten<U, V>(this U[] array, Func<U, V[]> func) {
        return array.Map(func).Flatten();
    }

    /// <summary>
    /// Check if any element in <paramref name="array"/> is a match for <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The type the elements in the array are of.</typeparam>
    /// <param name="array"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static bool Any<T>(this T[] array, Predicate<T> predicate) {
        for (int i = 0; i < array.Length; i++) {
            if (predicate(array[i])) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if the given array contains specified element and does NOT contain any of the other elements.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The array to check.</param>
    /// <param name="elem">The element to check if contained-</param>
    /// <param name="except">The elments that cannot be contained within the array.</param>
    /// <returns>If element is found and no instances from the except array is found, <see langword="true"/> is returned. Otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static bool ContainsWithout<T>(this T[] array, T elem, params T[] except)
        => array.Contains(elem) && !array.Any(x => except.Contains(x));

    /// <summary>
    /// Find the index of the first element matching the given <see cref="Predicate{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The array to find index in.</param>
    /// <param name="predicate">The predicate function to run on each element until returning true.</param>
    /// <returns>Will return -1 if no matching element is found. Otherwise the index of the first matching element.</returns>
    public static int IndexOf<T>(this T[] array, Predicate<T> predicate) {
        for (int i = 0; i < array.Length; i++) {
            if (predicate(array[i])) {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Append <paramref name="element"/> to the end of a copied <paramref name="array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The source array to append element to.</param>
    /// <param name="element">The element to append to the end of the source array.</param>
    /// <returns>A new array with <paramref name="element"/> as the last element.</returns>
    public static T[] Append<T>(this T[] array, T element) { // This should be faster than LINQ which possibly iterates over it and then appends (and will then require a .ToArray call)
        T[] buffer = new T[array.Length + 1];
        Array.Copy(array, buffer, array.Length);
        buffer[^1] = element;
        return buffer;
    }

    /// <summary>
    /// Prepends <paramref name="element"/> to the beginning of the <paramref name="array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The source array to prepend element to.</param>
    /// <param name="element">The elemend to prepend.</param>
    /// <returns>A copy of <paramref name="array"/> with <paramref name="element"/> as the first element.</returns>
    public static T[] Prepend<T>(this T[] array, T element) {
        T[] buffer = new T[array.Length + 1];
        buffer[0] = element;
        Array.Copy(array, 0, buffer, 1, array.Length);
        return buffer;
    }

    /// <summary>
    /// Concatenate two arrays.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The source array</param>
    /// <param name="other">The array to concatenate with.</param>
    /// <returns>A new array where the first elements are from <paramref name="array"/> and the last elements are from <paramref name="other"/>.</returns>
    public static T[] Concat<T>(this T[] array, T[] other) {
        T[] buffer = new T[array.Length + other.Length];
        Array.Copy(array, buffer, array.Length);
        Array.Copy(other, 0, buffer, array.Length, other.Length);
        return buffer;
    }

    /// <summary>
    /// Pick a random element from array.
    /// </summary>
    /// <typeparam name="T">The type the elements in the array are of.</typeparam>
    /// <param name="array">The array to pick element from</param>
    /// <param name="min">Smallest index in array to pick from.</param>
    /// <param name="max">Largest index in array to pick from.</param>
    /// <returns>A uniformly picked element from <paramref name="array"/>.</returns>
    public static T Random<T>(this T[] array, int min = 0, int max = int.MaxValue)
        => Random(array, BattlegroundsInstance.RNG, min, max);

    /// <summary>
    /// Pick a random element from array.
    /// </summary>
    /// <typeparam name="T">The type the elements in the array are of.</typeparam>
    /// <param name="array">The array to pick element from</param>
    /// <param name="random">The <see cref="System.Random"/> instance to get random index from.</param>
    /// <param name="min">Smallest index in array to pick from.</param>
    /// <param name="max">Largest index in array to pick from.</param>
    /// <returns>A uniformly picked element from <paramref name="array"/>.</returns>
    public static T Random<T>(this T[] array, Random random, int min = 0, int max = int.MaxValue) {
        
        // Check range
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), min, "Given minimum value cannot be smaller than 0.");

        // Update max
        max = max is int.MinValue ? array.Length : max;

        // Check range
        if (max > array.Length)
            throw new ArgumentOutOfRangeException(nameof(max), max, "Given maximum value cannot be greater than array length.");

        // Get index
        int i = random.Next(min, max);

        // Return 
        return array[i];
    
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static T[] Except<T>(this T[] array, T element) {
        int i = array.IndexOf(x => x?.Equals(element) ?? false);
        if (i == -1)
            return array;
        var before = array[0..i];
        var after = array[(i+1)..];
        return before.Concat(after);
    }

    /// <summary>
    /// Folds the input array using an accumulation function.
    /// </summary>
    /// <typeparam name="TSource">Source type to fold</typeparam>
    /// <typeparam name="TState">The folded state</typeparam>
    /// <param name="array">Array to fold into accumulation state.</param>
    /// <param name="initial">The initial state to fold from.</param>
    /// <param name="fun">Folder function.</param>
    /// <returns>The folded state</returns>
    public static TState Fold<TSource, TState>(this TSource[] array, TState initial, Func<TState, TSource, TState> fun) {
        var state = initial;
        for (int i = 0; i < array.Length; i++) {
            state = fun(state, array[i]);
        }
        return state;
    }

    /// <summary>
    /// Map an array into a <see cref="IDictionary{TKey, TValue}"/> instance.
    /// </summary>
    /// <typeparam name="T">The array type.</typeparam>
    /// <typeparam name="K">The lookup type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <param name="array">The array to map into a dictionary.</param>
    /// <param name="key">The key mapper function.</param>
    /// <param name="value">The value mapper function.</param>
    /// <returns>A <see cref="IDictionary{TKey, TValue}"/> instance with all array entries mapped according to <paramref name="key"/> and <paramref name="value"/> functions.</returns>
    public static IDictionary<K,V> ToLookup<T,K,V>(this T[] array, Func<T,K> key, Func<T,V> value) where K : notnull {
        var dict = new Dictionary<K,V>();
        for (int i = 0; i < array.Length; i++) {
            dict.Add(key(array[i]), value(array[i]));
        }
        return dict;
    }

    /// <summary>
    /// Map an array into a <see cref="IDictionary{TKey, TValue}"/> instance where a value member defines the lookup.
    /// </summary>
    /// <typeparam name="K">The lookup type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <param name="array">The array to map into a dictionary.</param>
    /// <param name="key">The key mapper function.</param>
    /// <returns>A <see cref="IDictionary{TKey, TValue}"/> instance with all array entries indexed according to <paramref name="key"/> function.</returns>
    public static IDictionary<K, V> ToLookup<K, V>(this V[] array, Func<V, K> key) where K : notnull
        => array.ToLookup(key, x => x);

    /// <summary>
    /// Finds the index with the smallest value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="array">The array to find smallest index in.</param>
    /// <returns>The index with the smallest instance in the array.</returns>
    public static int ArgMin<T>(this T[] array) where T : IComparable<T> {
        if (array.Length is 0) {
            return -1;
        }
        int j = 0;
        for (int i = 1; i < array.Length; i++) {
            if (array[i].CompareTo(array[j]) < 0) {
                j = i;
            }
        }
        return j;
    }

    /// <summary>
    /// Finds the index with the smallest value.
    /// </summary>
    /// <typeparam name="U">The value type.</typeparam>
    /// <typeparam name="V">The minimum value type.</typeparam>
    /// <param name="array">The array to find smallest index in.</param>
    /// <param name="min">The selector function.</param>
    /// <returns>The index with the smallest instance in the array.</returns>
    public static int ArgMin<U, V>(this U[] array, Func<U, V> min) where V : IComparable<V> {
        if (array.Length is 0) {
            return -1;
        }
        int j = 0;
        V v = min(array[0]);
        for (int i = 1; i < array.Length; i++) {
            var a = min(array[i]);
            if (a.CompareTo(v) < 0) {
                v = a;
                j = i;
            }
        }
        return j;
    }

    /// <summary>
    /// Finds the instance with the smallest value.
    /// </summary>
    /// <typeparam name="U">The value type.</typeparam>
    /// <typeparam name="V">The minimum value type.</typeparam>
    /// <param name="array">The array to find smallest instance in.</param>
    /// <param name="min">The selector function.</param>
    /// <returns>The instance with the smallest instance in the array.</returns>
    public static U Min<U, V>(this U[] array, Func<U, V> min) where V : IComparable<V>
        => array[ArgMin(array, min)];

    /// <summary>
    /// Finds the smallest value.
    /// </summary>
    /// <typeparam name="U">The value type.</typeparam>
    /// <typeparam name="V">The minimum value type.</typeparam>
    /// <param name="array">The array to find smallest value in.</param>
    /// <param name="min">The selector function.</param>
    /// <returns>The smallest in the array.</returns>
    public static V MinValue<U, V>(this U[] array, Func<U, V> min) where V : IComparable<V>
        => min(array.Min(min));

    /// <summary>
    /// Finds the index of the largest value.
    /// </summary>
    /// <typeparam name="U">The value type.</typeparam>
    /// <typeparam name="V">The maximum value type.</typeparam>
    /// <param name="array">The array to find largest index for.</param>
    /// <param name="max">The selector function.</param>
    /// <returns>The index with the largest instance in the array.</returns>
    public static int ArgMax<U, V>(this U[] array, Func<U, V> max) where V : IComparable<V> {
        if (array.Length is 0) {
            return -1;
        }
        int j = 0;
        V v = max(array[0]);
        for (int i = 1; i < array.Length; i++) {
            var a = max(array[i]);
            if (a.CompareTo(v) > 0) {
                v = a;
                j = i;
            }
        }
        return j;
    }

    /// <summary>
    /// Finds the index of the largest value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="array">The array to find largest index for.</param>
    /// <returns>The index with the largest instance in the array.</returns>
    public static int ArgMax<T>(this T[] array) where T : IComparable<T> {
        if (array.Length is 0) {
            return -1;
        }
        int j = 0;
        for (int i = 1; i < array.Length; i++) {
            if (array[i].CompareTo(array[j]) > 0) {
                j = i;
            }
        }
        return j;
    }

    /// <summary>
    /// Finds the instance with the largest value.
    /// </summary>
    /// <typeparam name="U">The value type.</typeparam>
    /// <typeparam name="V">The maximum value type.</typeparam>
    /// <param name="array">The array to find largest instance in.</param>
    /// <param name="max">The selector function.</param>
    /// <returns>The instance with the largest instance in the array.</returns>
    public static U Max<U, V>(this U[] array, Func<U, V> max) where V : IComparable<V>
        => array[ArgMax(array, max)];

    /// <summary>
    /// Finds the largest value.
    /// </summary>
    /// <typeparam name="U">The value type.</typeparam>
    /// <typeparam name="V">The maximum value type.</typeparam>
    /// <param name="array">The array to find largest value in.</param>
    /// <param name="max">The selector function.</param>
    /// <returns>The largest in the array.</returns>
    public static V MaxValue<U, V>(this U[] array, Func<U, V> max) where V : IComparable<V>
        => max(array.Max(max));

    /// <summary>
    /// Get the first matching instance or the <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The array instance type.</typeparam>
    /// <param name="array">The array to find element in.</param>
    /// <param name="predicate">The predicate to test.</param>
    /// <param name="defaultValue">The default value to return if no value is found.</param>
    /// <returns>The first <paramref name="predicate"/> matching instance. If none, <paramref name="defaultValue"/> is returned.</returns>
    public static T FirstOrDefault<T>(this T[] array, Predicate<T> predicate, T defaultValue) {
        for (int i = 0; i < array.Length; i++) {
            if (predicate(array[i]))
                return array[i];
        }
        return defaultValue;
    }

    /// <summary>
    /// Get the intersection (set of shared elements) between two arrays.
    /// </summary>
    /// <typeparam name="T">The array instance type.</typeparam>
    /// <param name="array">The first array to look through</param>
    /// <param name="other">The second array to intersect with.</param>
    /// <returns>The intersection between input arrays.</returns>
    public static T[] Intersect<T>(this T[] array, T[] other) where T : notnull {
        T[] values = new T[array.Length];
        int k = 0;
        for (int i = 0; i < array.Length; i++) {
            if (other.Any(x => x.Equals(array[i]))) {
                values[k++] = array[i];
            }
        }
        return values[..k];
    }

    /// <summary>
    /// Get the intersection (set of shared elements) between two arrays.
    /// </summary>
    /// <typeparam name="T">The array instance type.</typeparam>
    /// <param name="array">The first array to look through</param>
    /// <param name="other">The second array to intersect with.</param>
    /// <param name="equals">Function to run to check for equality (when true, elements are considered to be in the intersection).</param>
    /// <returns>The intersection between input arrays.</returns>
    public static T[] Intersect<T>(this T[] array, T[] other, Func<T, T, bool> equals) where T : notnull {
        T[] values = new T[array.Length];
        int k = 0;
        for (int i = 0; i < array.Length; i++) {
            for (int j = 0; j < other.Length; j++) {
                if (equals(array[i], other[j])) {
                    values[k++] = array[i];
                    break;
                }
            }
        }
        return values[..k];
    }

}
