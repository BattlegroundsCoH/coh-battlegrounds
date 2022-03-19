using System;
using System.ComponentModel;

using BattlegroundsApp.Controls;
using BattlegroundsApp.MVVM;

namespace BattlegroundsApp.Utilities;

public class CapacityValue : ILocLabelArgumentsObject, INotifyPropertyChanged {

    private Func<int> m_eval;
    private int m_value;
    private int m_backerValue;

    public event ObjectChangedEventHandler ObjectChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public int Current {
        get => this.m_eval is null ? this.m_value : this.m_eval.Invoke();
        set => this.m_value = value;
    }

    public int Capacity { get; set; }

    public bool IsAtCapacity => this.Current == this.Capacity;

    public CapacityValue(int curr, int max, Func<int> eval = null) {
        this.Current = curr;
        this.Capacity = max;
        this.m_eval = eval;
    }

    public CapacityValue(int max, Func<int> eval) {
        this.Current = eval();
        this.Capacity = max;
        this.m_eval = eval;
    }

    public void BindCurrent(Func<int> eval)
        => this.m_eval = eval;

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

    public bool InBounds(int add) => this.Current + add <= this.Capacity;

    public object[] ToArgs() => new object[] { Current, Capacity };

    public override string ToString() => $"{Current}/{Capacity}";

}
