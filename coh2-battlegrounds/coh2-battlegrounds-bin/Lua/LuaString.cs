namespace Battlegrounds.Lua {

    /// <summary>
    /// Class for representing a Lua string value. Concrete implementation of <see cref="LuaValue"/>.
    /// </summary>
    public class LuaString : LuaValue {

        private readonly string m_internalStringValue;

        /// <summary>
        /// Represents the empty <see cref="LuaString"/>. This is a read-only field.
        /// </summary>
        public static readonly LuaString Empty = new(string.Empty);

        /// <summary>
        /// Get the length of the string.
        /// </summary>
        public int Length => this.m_internalStringValue.Length;

        /// <summary>
        /// Initialize a new <see cref="LuaString"/> class with a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The actual string value to be represented by the <see cref="LuaString"/>.</param>
        public LuaString(string value) => this.m_internalStringValue = value;

        /// <summary>
        /// Get numeric value of the string.
        /// </summary>
        /// <returns>If string represents a numeric value, a <see cref="LuaNumber"/> value of that numeric; Otherwise, <see cref="LuaNil"/>.</returns>
        public LuaValue Num() => double.TryParse(this.m_internalStringValue, out double n) ? new LuaNumber(n) : LuaNil.Nil;

        public override bool Equals(LuaValue value) => value is LuaString s && s.m_internalStringValue == this.m_internalStringValue;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override string Str() => this.m_internalStringValue;

        public override int GetHashCode() => this.m_internalStringValue.GetHashCode();

        public override string ToString() => this.m_internalStringValue;

        public override LuaType GetLuaType() => LuaType.LUA_STRING;

    }

}
