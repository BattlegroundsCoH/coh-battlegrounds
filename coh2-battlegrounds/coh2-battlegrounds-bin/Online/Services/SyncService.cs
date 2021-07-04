using System;
using System.Threading;
using System.Threading.Tasks;

namespace Battlegrounds.Online.Services {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isDone"></param>
    /// <param name="pulseNumber"></param>
    public delegate void WaitPulse(bool isDone, int pulseNumber);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pulseNumber"></param>
    /// <returns></returns>
    public delegate bool WaitPulsePredicate(int pulseNumber);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public delegate bool SyncPredicate();

    /// <summary>
    /// 
    /// </summary>
    public static class SyncService {
    
        /// <summary>
        /// Timeout object marking whether a <see cref="SyncService"/> "wait" method timed out.
        /// </summary>
        public struct TimedOut {
            private readonly bool m_timeout;
            public TimedOut(bool yes) {
                this.m_timeout = yes;
            }
            /// <summary>
            /// Invoke action if timeout.
            /// </summary>
            /// <param name="action">The action to invoke if timed-out.</param>
            /// <returns>Will return <see langword="true"/> if timed out. Otherwise <see langword="false"/>.</returns>
            public bool Then(Action action) {
                if (this.m_timeout) {
                    action?.Invoke();
                }
                return this.m_timeout;
            }
            public static implicit operator bool(TimedOut timedOut) => timedOut.m_timeout;
        }

        /// <summary>
        /// Wait until a <paramref name="predicate"/> condition is met.
        /// </summary>
        /// <remarks>
        /// Condition check interval and timeout counter can be specified.
        /// </remarks>
        /// <param name="predicate">The synchronization predicate to run at each check interval.</param>
        /// <param name="step">The interval in milliseconds between calls.</param>
        /// <param name="max">The max intervals before timing out. -1 means never time out.</param>
        /// <returns>Will return a <see cref="TimedOut"/> objec that can handle events in case the waiting timed out.</returns>
        public static TimedOut WaitUntil(SyncPredicate predicate, int step = 100, int max = -1) {
            int counter = 0;
            while (!predicate() && (max != -1 && counter < max)) {
                Thread.Sleep(step);
                if (max != -1) {
                    counter++;
                }
            }
            return new TimedOut(counter >= max && max != -1);
        }

        /// <summary>
        /// Wait until a <paramref name="predicate"/> condition is met and invoke the <paramref name="pulse"/> handler at each check.
        /// </summary>
        /// <param name="predicate">The synchronization predicate to run at each check interval.</param>
        /// <param name="pulse">The pulse method to invoke at each interval</param>
        /// <param name="max">The max intervals before timing out. Must be set.</param>
        /// <param name="step">The interval in milliseconds between calls.</param>
        /// <returns></returns>
        public static TimedOut WaitAndPulseUntil(SyncPredicate predicate, WaitPulse pulse, uint max, int step = 100) {
            int counter = 0;
            while (!predicate() && counter < max) {
                Thread.Sleep(step);
                pulse?.Invoke(false, counter++);
            }
            pulse?.Invoke(true, counter);
            return new TimedOut(counter >= max);
        }

        /// <summary>
        /// Wait until a <paramref name="predicate"/> condition is met or if the <paramref name="pulse"/> predicate condition is met at each check.
        /// </summary>
        /// <param name="predicate">The synchronization predicate to run at each check interval.</param>
        /// <param name="pulse">The pulse method to invoke at each interval. If <see langword="true"/> is returned, the execution will continue.</param>
        /// <param name="max">The max intervals before timing out. Must be set.</param>
        /// <param name="step">The interval in milliseconds between calls.</param>
        /// <returns></returns>
        public static TimedOut WaitAndPulseUntil(SyncPredicate predicate, WaitPulsePredicate pulse, uint max, int step = 100) {
            int counter = 0;
            while (!predicate() && !pulse(counter++) && counter < max) {
                Thread.Sleep(step);
            }
            pulse(counter);
            return new TimedOut(counter >= max);
        }

        /// <summary>
        /// Wait until a specified condition is met. Condition check interval and timeout counter can be specified.
        /// </summary>
        /// <param name="predicate">The synchronization predicate to run at each check interval.</param>
        /// <param name="step">The interval in milliseconds between calls.</param>
        /// <param name="max">The max intervals before timing out. -1 means never time out.</param>
        /// <returns>Will return a <see cref="TimedOut"/> objec that can handle events in case the waiting timed out.</returns>
        public static async Task<TimedOut> WaitUntilAsync(SyncPredicate predicate, int step = 100, int max = -1) {
            int counter = 0;
            while (!predicate() && (max != -1 && counter < max)) {
                await Task.Delay(step);
                if (max != -1) {
                    counter++;
                }
            }
            return new TimedOut(counter >= max && max != -1);
        }

    }

}
