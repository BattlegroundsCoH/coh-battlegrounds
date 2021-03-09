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

        public abstract override int GetHashCode();

        /// <summary>
        /// Get the <see cref="LuaType"/> of the value.
        /// </summary>
        /// <returns>A <see cref="LuaType"/> instance representing the Lua type.</returns>
        public abstract LuaType GetLuaType();

        /// <summary>
        /// Convert a managed object into a proper <see cref="LuaValue"/> representation.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to get <see cref="LuaValue"/> representation of.</param>
        /// <returns>The best <see cref="LuaValue"/> representation of <paramref name="value"/>. If no proper representation could be found a <see cref="LuaUserObject"/> is used.</returns>
        public static LuaValue ToLuaValue(object value) => value switch {
            double d => new LuaNumber(d),
            float f => new LuaNumber(f),
            int i => new LuaNumber(i),
            string s => new LuaString(s),
            bool b => new LuaBool(b),
            null => new LuaNil(),
            _ => new LuaUserObject(value)
        };

    }

}
