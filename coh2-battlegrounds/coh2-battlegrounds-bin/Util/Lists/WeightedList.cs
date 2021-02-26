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
            var pairs = this.Select(x => x).OrderBy(x => x.Value).ToList();

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
    
    }

}
