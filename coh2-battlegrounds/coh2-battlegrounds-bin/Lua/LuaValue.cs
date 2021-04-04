namespace Battlegrounds.Lua {

    /// <summary>
    /// Abstract representation of some value in a Lua environment.
    /// </summary>
    public abstract class LuaValue {

        /// <summary>
        /// Convert the value into its <see cref="string"/> representation.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of the <see cref="LuaValue"/>.</returns>
        public abstract string Str();

        /// <summary>
        /// Determine if the two <see cref="LuaValue"/> objects can be considered equal.
        /// </summary>
        /// <param name="value">The <see cref="LuaValue"/> to compare equivalence with.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public abstract bool Equals(LuaValue value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>() where T : LuaValue => (T)this;

        public abstract override int GetHashCode();

        /// <summary>
        /// Get the <see cref="LuaType"/> of the value.
        /// </summary>
        /// <returns>A <see cref="LuaType"/> instance representing the Lua type.</returns>
        public abstract LuaType GetLuaType();

        /// <summary>
        /// Convert <paramref name="v"/> into the proper <see cref="LuaValue"/> representation.
        /// </summary>
        /// <param name="v">The value to convert.</param>
        public static implicit operator LuaValue(double v) => new LuaNumber(v);

        /// <summary>
        /// Convert <paramref name="v"/> into the proper <see cref="LuaValue"/> representation.
        /// </summary>
        /// <param name="v">The value to convert.</param>
        public static implicit operator LuaValue(int v) => new LuaNumber(v);

    }

}
