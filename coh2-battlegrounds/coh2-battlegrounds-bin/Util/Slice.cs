using System;
using System.Collections;
using System.Collections.Generic;

namespace Battlegrounds.Util;

/// <summary>
/// Represents a slice of an array from a start position to an end position.
/// </summary>
/// <typeparam name="T">The slice element type.</typeparam>
public readonly struct Slice<T> : IEnumerable<T> {

    private readonly T[] m_source;
    private readonly int m_pos;
    private readonly int m_end;

    /// <summary>
    /// Get the length of the slice.
    /// </summary>
    public int Length => this.m_end - this.m_pos;

    /// <summary>
    /// Initialise a new slice instance, slicing data from <paramref name="source"/> starting at <paramref name="startIndex"/> and ending at <paramref name="endIndex"/>.
    /// </summary>
    /// <param name="source">The source array to slice data from.</param>
    /// <param name="startIndex">The index of the start position of the slice.</param>
    /// <param name="endIndex">The end index of the slice.</param>
    /// <exception cref="IndexOutOfRangeException"/>
    public Slice(T[] source, int startIndex, int endIndex) {
        
        // Check end index
        if (endIndex > source.Length) {
            throw new IndexOutOfRangeException($"End-Index {endIndex} is out of bounds (length = {source.Length}).");
        }
        
        // Check start index
        if (startIndex < 0) {
            throw new IndexOutOfRangeException($"Start-Index {startIndex} is out of bounds; index cannot be less than 0.");
        }
        
        // Check index order
        if (startIndex > endIndex) {
            throw new IndexOutOfRangeException($"Start-Index {startIndex} is out of bounds; index cannot be greater than end-index ({endIndex}).");
        }
        
        // Set fields
        this.m_source = source;
        this.m_pos = startIndex;
        this.m_end = endIndex;

    }

    /// <summary>
    /// Get or set element in slice.
    /// </summary>
    /// <param name="index">The index to pick element from.</param>
    /// <returns>The element specified within the slice range.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public T this[int index] {
        get {
            int i = this.m_pos + index;
            if (i >= 0 && i < this.m_source.Length) {
                return this.m_source[i];
            } else {
                throw new IndexOutOfRangeException();
            }
        }
        set {
            int i = this.m_pos + index;
            if (i >= 0 && i < this.m_source.Length) {
                this.m_source[i] = value;
            } else {
                throw new IndexOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Get the slice as a new array.
    /// </summary>
    /// <returns>Fresh array containing elements from source array within slice range.</returns>
    public T[] GetSlice() {
        T[] slice = new T[this.Length];
        Array.Copy(this.m_source, this.m_pos, slice, 0, this.Length);
        return slice;
    }

    /// <summary>
    /// Returns an enumerator that enumerates through the <see cref="Slice{T}"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="Slice{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator() {
        return new List<T>(this.GetSlice()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetSlice().GetEnumerator();
    }

    /// <summary>
    /// Implicit conversion from slice to array.
    /// </summary>
    /// <param name="slice"></param>
    public static implicit operator T[](Slice<T> slice)
        => slice.GetSlice();

}

/// <summary>
/// Static extension class for adding a .Slice to all array types.
/// </summary>
public static class Slicing {

    /// <summary>
    /// Create a new slice of an array from specified <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <typeparam name="T">The slice element type.</typeparam>
    /// <param name="array">The array to slice data from.</param>
    /// <param name="start">Start index of slice.</param>
    /// <param name="end">End index of slice.</param>
    /// <returns>A slice of the <paramref name="array"/>.</returns>
    public static Slice<T> Slice<T>(this T[] array, int start, int end)
        => new(array, start, end);

}
