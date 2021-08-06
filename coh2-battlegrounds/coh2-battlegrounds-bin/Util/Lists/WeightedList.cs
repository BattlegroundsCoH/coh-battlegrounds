using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Util.Lists {

    /// <summary>
    /// Represents a list of values bound to a <see cref="double"/> weight in the range [0, 1].
    /// </summary>
    /// <typeparam name="T">The value stored in the list.</typeparam>
    public class WeightedList<T> : Dictionary<T, double> {

        /// <summary>
        /// Initialize a new and empty instance of a <see cref="WeightedList{T}"/> class.
        /// </summary>
        public WeightedList() : base() { }

        /// <summary>
        /// Initialize a new <see cref="WeightedList{T}"/> with elements from <paramref name="source"/> and weighted by <paramref name="weighter"/>.
        /// </summary>
        /// <param name="source">The source collection to draw elements from.</param>
        /// <param name="weighter">The weighting function to invoke on each element.</param>
        public WeightedList(IEnumerable<T> source, Func<T, double> weighter) : base() {
            var e = source.GetEnumerator();
            while (e.MoveNext()) {
                this.Add(e.Current, weighter(e.Current));
            }
        }

        /// <summary>
        /// Pick a random element from the list based on assigned weights.
        /// </summary>
        /// <returns>A randomly picked <typeparamref name="T"/> object.</returns>
        /// <exception cref="InvalidOperationException"/>
        public T Pick() => this.Pick(new Random());

        /// <summary>
        /// Pick a random element from the list based on assigned weights.
        /// </summary>
        /// <param name="random">The <see cref="Random"/> instance to use for generating random values.</param>
        /// <returns>A randomly picked <typeparamref name="T"/> object.</returns>
        /// <exception cref="InvalidOperationException"/>
        public T Pick(Random random) {

            // Make sure we have elements to pick from
            if (this.Count == 0)
                throw new InvalidOperationException("Cannot pick a random element from an empty list.");

            // Select self
            var pairs = this.OrderBy(x => x.Value).ToList();

            // While there's more than 1
            while (pairs.Count > 1) {
                double r = random.NextDouble();
                if (r <= pairs[0].Value) {
                    return pairs[0].Key;
                } else {
                    pairs.RemoveAt(0);
                }
            }

            //  Return last element
            return pairs[0].Key;

        }

        /// <summary>
        /// Pick a random element from and then remove it.
        /// </summary>
        /// <returns>A randomly picked <typeparamref name="T"/> object.</returns>
        /// <exception cref="InvalidOperationException"/>
        public T PickOut() => this.PickOut(new Random());

        /// <summary>
        /// Pick a random element from and then remove it.
        /// </summary>
        /// <param name="random">The random generator to use when picking random values.</param>
        /// <returns>A randomly picked <typeparamref name="T"/> object.</returns>
        /// <exception cref="InvalidOperationException"/>
        public T PickOut(Random random) {
            var pick = this.Pick(random);
            this.Remove(pick);
            return pick;
        }

        /// <summary>
        /// Get a copy of the <see cref="WeightedList{T}"/> as <see cref="List{T}"/>.
        /// </summary>
        /// <remarks>
        /// This will drop the weights.
        /// </remarks>
        /// <returns>A <see cref="List{T}"/> consisting of elements from the <see cref="WeightedList{T}"/>.</returns>
        public List<T> ToList() {
            List<T> elements = new List<T>(this.Count);
            var e = this.GetEnumerator();
            while (e.MoveNext()) {
                elements.Add(e.Current.Key);
            }
            return elements;
        }

    }

    /// <summary>
    /// Static helper class for offering extension methods related to the <see cref="WeightedList{T}"/> class.
    /// </summary>
    public static class WeightedListExtension {

        /// <summary>
        /// Convert <see cref="IEnumerable{T}"/> to a weighted list where each element in <paramref name="enumerable"/> is weighted by <paramref name="weightFunction"/>.
        /// </summary>
        /// <typeparam name="T">The generic object that is to be weighted.</typeparam>
        /// <param name="enumerable">The enumerable to convert into a weighted list.</param>
        /// <param name="weightFunction">The weight function to apply on each element in <paramref name="enumerable"/></param>
        /// <returns>A <see cref="WeightedList{T}"/> consisting of <paramref name="enumerable"/> elements.</returns>
        public static WeightedList<T> ToWeightedList<T>(this IEnumerable<T> enumerable, Func<T, double> weightFunction) {
            var e = enumerable.GetEnumerator();
            WeightedList<T> list = new WeightedList<T>();
            while (e.MoveNext()) {
                list.Add(e.Current, weightFunction(e.Current));
            }
            return list;
        }

    }

}
