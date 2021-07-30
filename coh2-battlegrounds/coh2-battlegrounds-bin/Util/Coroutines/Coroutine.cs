using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Battlegrounds.Functional;

namespace Battlegrounds.Util.Coroutines {

    /// <summary>
    /// Utility class for running <see cref="Coroutine"/> functionality.
    /// </summary>
    public static class Coroutine {

        private class CoroutineMetaData {

            public IEnumerator Enumerator { get; }
            public ICoroutineDispatcher Dispatcher { get; }
            public DateTime StartTime { get; }
            public int CoroutineID => this.Enumerator.GetHashCode();

            private YieldInstruction currentYield;

            public CoroutineMetaData(IEnumerator enumerator, ICoroutineDispatcher dispatcher) {
                this.Enumerator = enumerator;
                this.Dispatcher = dispatcher;
                this.StartTime = DateTime.Now;
            }

            public bool Update() {
                if (currentYield is not null) {
                    if (!currentYield.CanAdvance()) {
                        return true;
                    }
                }
                bool shouldContinue = this.Enumerator.MoveNext();
                if (shouldContinue) {
                    if (this.Enumerator.Current is YieldInstruction instruction) {
                        this.currentYield = instruction;
                    } else {
                        this.currentYield = null;
                    }
                }
                return shouldContinue;
            }
        }

        private static List<CoroutineMetaData> __runningCoroutines;
        private static CancellationTokenSource __cancelTokenSource;
        private static Task __coroutineTask;
        private static bool __doCoroutines;

        private const int __RefreshRate = (int)(1 / 60.0 * 1000);

        /// <summary>
        /// Start a new coroutine.
        /// </summary>
        /// <param name="enumerator">The <see cref="Coroutine"/> object to start.</param>
        /// <returns>The ID associated with the <see cref="Coroutine"/>.</returns>
        public static int StartCoroutine(IEnumerator enumerator)
            => StartCoroutine(enumerator, ICoroutineDispatcher.CurrentDispatcher);

        /// <summary>
        /// Start a new coroutine using the <paramref name="dispatcher"/> as execution thread.
        /// </summary>
        /// <param name="enumerator">The <see cref="Coroutine"/> object to start.</param>
        /// <param name="dispatcher">The <see cref="Dispatcher"/> to invoke when updating coroutine.</param>
        /// <returns>The ID associated with the <see cref="Coroutine"/>.</returns>
        public static int StartCoroutine(IEnumerator enumerator, ICoroutineDispatcher dispatcher) {
            if (__runningCoroutines is null) {
                __runningCoroutines = new List<CoroutineMetaData>();
            }
            lock (__runningCoroutines) {
                __runningCoroutines.Add(new CoroutineMetaData(enumerator, dispatcher));
            }
            if (__coroutineTask is null || __coroutineTask.Status == TaskStatus.RanToCompletion) {
                __cancelTokenSource = new CancellationTokenSource();
                __doCoroutines = true;
                __coroutineTask = new Task(UpdateCoroutines, __cancelTokenSource.Token, TaskCreationOptions.LongRunning);
                __coroutineTask.Start();
                //Trace.WriteLine($"Starting coroutine engine with refresh rate ~{__RefreshRate}ms", nameof(Coroutine));
            }
            int corID = enumerator.GetHashCode();
            //Trace.WriteLine($"Starting new coroutine 0x{corID:X8}", nameof(Coroutine));
            return corID;
        }

        /// <summary>
        /// Update all registered coroutines.
        /// </summary>
        public static void UpdateCoroutines() {
            while (__doCoroutines && __runningCoroutines.Count > 0) {
                var itt = __runningCoroutines.GetSafeEnumerator();
                while (itt.MoveNext()) {
                    bool rem = false;
                    itt.Current.Dispatcher.Invoke(() => {
                        if (!itt.Current.Update()) {
                            ///Trace.WriteLine($"Finished coroutine 0x{itt.Current.CoroutineID:X8} (ran for {(DateTime.Now - itt.Current.StartTime).TotalSeconds}s)", nameof(Coroutine));
                            rem = true;
                        }
                    });
                    if (rem)
                        __runningCoroutines.Remove(itt.Current);
                }
                Thread.Sleep(__RefreshRate);
            }
        }

        /// <summary>
        /// Halt the coroutine with specified ID
        /// </summary>
        /// <param name="coroutineID">The ID of the coroutine to halt.</param>
        /// <returns>If coroutine was found and halted, <see langword="true"/> is returned. Otherwise <see langword="false"/>.</returns>
        public static bool HaltCoroutine(int coroutineID) {
            var metadata = __runningCoroutines.FirstOrDefault(x => x.CoroutineID == coroutineID);
            if (metadata is not null) {
                __runningCoroutines.Remove(metadata);
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Halt all currently running coroutines.
        /// </summary>
        public static void HaltAllCoroutines() => __cancelTokenSource?.Cancel();

    }

}
