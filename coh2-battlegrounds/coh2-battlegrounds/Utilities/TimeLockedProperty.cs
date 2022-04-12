using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp.Utilities {

    public class TimeLockedProperty<T> : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private T m_value;
        private DateTime m_lastUpdate;

        public double TimeThreshold { get; }

        public bool IsLocked => (DateTime.Now - this.m_lastUpdate).TotalMilliseconds < TimeThreshold;

        public T Value {
            get => this.m_value;
            set {
                if (this.IsLocked) {
                    return;
                }
                this.m_value = value;
                this.m_lastUpdate = DateTime.Now;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public TimeLockedProperty(TimeSpan timeLock) {
            this.TimeThreshold = timeLock.TotalMilliseconds;
            this.m_value = default;
        }

        public TimeLockedProperty(double timeLockMilliseconds) {
            this.TimeThreshold = timeLockMilliseconds;
            this.m_value = default;
        }

        public TimeLockedProperty(T initialValue, TimeSpan timeLock) {
            this.TimeThreshold = timeLock.TotalMilliseconds;
            this.Value = initialValue;
        }

        public TimeLockedProperty(T initialValue, double timeLockMilliseconds) {
            this.TimeThreshold = timeLockMilliseconds;
            this.Value = initialValue;
        }

        public TimeLockedProperty(T initialValue, TimeSpan timeLock, PropertyChangedEventHandler propertyChangedEvent) {
            this.TimeThreshold = timeLock.TotalMilliseconds;
            this.PropertyChanged += propertyChangedEvent;
            this.Value = initialValue;
        }

        public TimeLockedProperty(T initialValue, double timeLockMilliseconds, PropertyChangedEventHandler propertyChangedEvent) {
            this.TimeThreshold = timeLockMilliseconds;
            this.PropertyChanged += propertyChangedEvent;
            this.Value = initialValue;
        }

        public void ForceSetvalue(T val) {
            this.m_value = val;
            this.m_lastUpdate = DateTime.Now;
        }

        public static implicit operator T(TimeLockedProperty<T> property)
            => property.Value;

    }

}
