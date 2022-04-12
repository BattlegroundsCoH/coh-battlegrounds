using System.Collections.Generic;

namespace Battlegrounds.Util.Lists {

    /// <summary>
    /// List of distinct indexable elements. Extends <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DistinctList<T> : List<T> {

        public new void Add(T item) {
            if (!this.Contains(item)) {
                base.Add(item);
            }
        }

    }

}
