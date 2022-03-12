using System;

namespace Battlegrounds.Util;

/// <summary>
/// Reference to a value.
/// </summary>
/// <typeparam name="T">The type of value being referenced.</typeparam>
public class ValRef<T> where T : struct {
    
    private T m_value;

    /// <summary>
    /// Get or set the value being referenced.
    /// </summary>
    public T Value {
        get { return this.m_value; }
        set { this.m_value = value; }
    }

    /// <summary>
    /// Create a new reference instance.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public ValRef(T value) {
        this.m_value = value;
    }

    /// <summary>
    /// Modify the referenced value.
    /// </summary>
    /// <param name="func">Mutator function to apply on referenced value.</param>
    /// <returns>The new value.</returns>
    public T Change(Func<T, T> func)
        => this.m_value = func(this.m_value);

    /// <summary>
    /// Create a reference to the specified value.
    /// </summary>
    /// <param name="value">The value to reference.</param>
    public static implicit operator ValRef<T>(T value) => new(value);

    /// <summary>
    /// Dereference the referenced value.
    /// </summary>
    /// <param name="refval">The reference value to dereference.</param>
    public static implicit operator T(ValRef<T> refval) => refval.m_value;

}
