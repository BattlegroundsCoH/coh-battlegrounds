using System;
using System.ComponentModel;

using Battlegrounds.Data;
using Battlegrounds.Misc.Locale;

namespace Battlegrounds.Misc.Values;

/// <summary>
/// 
/// </summary>
public class CapacityValue : ILocaleArgumentsObject, INotifyPropertyChanged {

    private Func<int>? m_eval;
    private int m_value;
    private int m_backerValue;

    public event ObjectChangedEventHandler? ObjectChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    public int Current {
        get => this.m_eval is null ? this.m_value : this.m_eval.Invoke();
        set => this.m_value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsAtCapacity => this.Current >= this.Capacity;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="curr"></param>
    /// <param name="max"></param>
    /// <param name="eval"></param>
    public CapacityValue(int curr, int max, Func<int>? eval = null) {
        this.Current = curr;
        this.Capacity = max;
        this.m_eval = eval;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="max"></param>
    /// <param name="eval"></param>
    public CapacityValue(int max, Func<int> eval) {
        this.Current = eval();
        this.Capacity = max;
        this.m_eval = eval;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eval"></param>
    public void BindCurrent(Func<int> eval)
        => this.m_eval = eval;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
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
