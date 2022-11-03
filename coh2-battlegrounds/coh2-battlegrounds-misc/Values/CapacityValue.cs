using System;
using System.ComponentModel;

using Battlegrounds.Data;
using Battlegrounds.Misc.Locale;

namespace Battlegrounds.Misc.Values;

/// <summary>
/// Class representing a value with a maximum obtainable value.
/// </summary>
public sealed class CapacityValue : ILocaleArgumentsObject, INotifyPropertyChanged {

    private Func<int>? m_eval;
    private int m_value;
    private int m_backerValue;

    public event ObjectChangedEventHandler? ObjectChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Get or set the current value. Does not verify if new value is within capacity value.
    /// </summary>
    public int Current {
        get => this.m_eval is null ? this.m_value : this.m_eval.Invoke();
        set => this.m_value = value;
    }

    /// <summary>
    /// Get or set the capacity value.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Get if the value is equal to the capacity value.
    /// </summary>
    public bool IsAtCapacity => this.Current >= this.Capacity;

    /// <summary>
    /// Initialise a new <see cref="CapacityValue"/> instance with a current value and a maximum value.
    /// </summary>
    /// <param name="curr">The current value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="eval">The evaluation function to fetch updated values.</param>
    public CapacityValue(int curr, int max, Func<int>? eval = null) {
        this.Current = curr;
        this.Capacity = max;
        this.m_eval = eval;
    }

    /// <summary>
    /// Initialise a new <see cref="CapacityValue"/> instance with a maximum value and a function to retrieve the current value.
    /// </summary>
    /// <param name="max">The maximum value.</param>
    /// <param name="eval">The evaluation function to fetch updated values.</param>
    public CapacityValue(int max, Func<int> eval) {
        this.Current = eval();
        this.Capacity = max;
        this.m_eval = eval;
    }

    /// <summary>
    /// Set the function that evaluates the current value.
    /// </summary>
    /// <param name="eval">The new evaluation function.</param>
    public void BindCurrent(Func<int> eval)
        => this.m_eval = eval;

    /// <summary>
    /// Update the capacity value.
    /// </summary>
    /// <param name="sender">The object that triggered the update.</param>
    public void Update(object sender) {
        if (this.m_eval is not null) {
            int next = this.Current;
            if (this.Current != this.m_backerValue) {
                this.ObjectChanged?.Invoke(sender, this);
                this.m_backerValue = next;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Current)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAtCapacity)));
            }
        }
    }

    /// <summary>
    /// Test if the capacity is still within bounds if the specified <paramref name="amount"/> of elements are added.
    /// </summary>
    /// <param name="amount">The amount of elements to add to existing elements to test capacity.</param>
    /// <returns>When the computed test value is within the capacity, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool Test(int amount) => this.Current + amount <= this.Capacity;

    public object[] ToArgs() => new object[] { Current, Capacity };

    public override string ToString() => $"{Current}/{Capacity}";

}
