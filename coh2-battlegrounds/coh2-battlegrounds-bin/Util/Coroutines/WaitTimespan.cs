using System;

namespace Battlegrounds.Util.Coroutines {

    public sealed class WaitTimespan : YieldInstruction {

        DateTime m_beginAt;
        TimeSpan m_time;

        public WaitTimespan(TimeSpan span) {
            this.m_time = span;
            this.m_beginAt = DateTime.Now;
        }

        public override bool CanAdvance()
            => DateTime.Now - this.m_beginAt > this.m_time;

    }

}
