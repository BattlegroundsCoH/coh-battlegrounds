namespace Battlegrounds.Util.Coroutines {

    /// <summary>
    /// Yield handler for a <see cref="Coroutine"/> where execution will halt until next coroutine pass.
    /// </summary>
    public class WaitCycle : YieldInstruction {

        bool m_cycleFlag;

        /// <summary>
        /// Initialize a new <see cref="WaitCycle"/> class.
        /// </summary>
        public WaitCycle() => this.m_cycleFlag = false;

        public override bool CanAdvance() {
            if (this.m_cycleFlag) {
                return true;
            } else {
                this.m_cycleFlag = true;
                return false;
            }
        }

    }

}
