using System;
using System.Collections.ObjectModel;

namespace Battlegrounds.Misc.Collections;

/// <summary>
/// Class extension of <see cref="ObservableCollection{T}"/> adding a Tag property.
/// </summary>
/// <typeparam name="T">The collection element type.</typeparam>
public class TaggedObservableCollection<T> : ObservableCollection<T> {

    private object? m_tagValue;

    /// <summary>
    /// Event triggered when the tag is changed.
    /// </summary>
    public event EventHandler<object?>? TagChanged;

    /// <summary>
    /// Get or set the tag associated with the collection.
    /// </summary>
    public object? Tag {
        get => this.m_tagValue;
        set {
            this.m_tagValue = value;
            this.TagChanged?.Invoke(this, value);
        }
    }

}
