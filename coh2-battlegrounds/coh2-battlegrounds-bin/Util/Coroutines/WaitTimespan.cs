using System;

namespace Battlegrounds.Util.Coroutines {

    /// <summary>
    /// Yield handler for a <see cref="Coroutine"/> where execution will halt until specified time has passed.
    /// </summary>
    public sealed class WaitTimespan : YieldInstruction {

        DateTime m_beginAt;
        TimeSpan m_time;

        /// <summary>
        /// Initialize a new <see cref="WaitTimespan"/> class with <paramref name="span"/> to wait.
        /// </summary>
        /// <param name="span">The amount of time to wait before resuming execution.</param>
        public WaitTimespan(TimeSpan span) {
            this.m_time = span;
            this.m_beginAt = DateTime.Now;
        }

        public override bool CanAdvance()
            => DateTime.Now - this.m_beginAt > this.m_time;

    }

}
