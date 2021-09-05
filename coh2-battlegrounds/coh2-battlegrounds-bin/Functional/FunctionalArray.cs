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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] Merge<T>(this T[][] array) {
            List<T> ts= new List<T>(array.Length);
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
        public static V[] MapAndMerge<U, V>(this U[] array, Func<U, V[]> func) {
            V[][] non_merged = array.Map(func);
            int len = non_merged.Aggregate(0, (a, b) => a + b.Length);
            V[] merged = new V[len];
            int i = 0;
            foreach (var a in non_merged) {
                for (int j = 0; j < a.Length; j++) {
                    merged[i++] = a[j];
                }
            }
            return merged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

        private static readonly Random __funcRandom = new();

        public static T Random<T>(this T[] array)
            => Random(array, __funcRandom);

        public static T Random<T>(this T[] array, Random random) {
            return array[random.Next(0, array.Length)];
        }

    }

}
