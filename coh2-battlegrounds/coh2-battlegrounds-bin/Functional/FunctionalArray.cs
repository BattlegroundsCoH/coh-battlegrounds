using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Functional {

    /// <summary>
    /// Functional helper functions for arrays.
    /// </summary>
    public static class FunctionalArray {

        /// <summary>
        /// Loop through each element in an array and execute a function that can mutate the element.
        /// </summary>
        /// <typeparam name="T">The type the array is of.</typeparam>
        /// <param name="array">The array to run method on.</param>
        /// <param name="func">The function to run on each element.</param>
        /// <returns>A new array consisting of the elements returned through the given function.</returns>
        public static T[] ForEach<T>(this T[] array, Func<T, T> func) {
            T[] t = new T[array.Length];
            for (int i = 0; i < array.Length; i++) {
                t[i] = func(array[i]);
            }
            return t;
        }

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
            => Random(array, BattlegroundsInstance.RNG);

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

    }

}
