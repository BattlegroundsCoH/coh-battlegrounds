namespace Battlegrounds.Util.Coroutines {
    
    /// <summary>
    /// Abstract class representing an instruction to evaluate while yielding.
    /// </summary>
    public abstract class YieldInstruction {

        /// <summary>
        /// Determines if instruction allows advancements.
        /// </summary>
        /// <returns>Will return <see langword="true"/> if <see cref="Coroutine"/> should advance. Otherwise <see langword="false"/></returns>.
        public abstract bool CanAdvance();

    }

}
