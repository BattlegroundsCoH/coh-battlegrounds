using System.Collections.Generic;

namespace Battlegrounds.Misc.Collections;

/// <summary>
/// List of distinct indexable elements. Extends <see cref="List{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DistinctList<T> : List<T> {

    /// <summary>
    /// Adds an object to the end of the <see cref="DistinctList{T}"/> if item is not already in the <see cref="DistinctList{T}"/>.
    /// </summary>
    /// <param name="item">The item to append.</param>
    /// <returns>If item is added, <see langword="true"/>; Otherwise if already in the list, <see langword="false"/>.</returns>
    public new bool Add(T item) {
        if (!this.Contains(item)) {
            base.Add(item);
            return true;
        }
        return false;
    }

}
