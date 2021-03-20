namespace Battlegrounds.Lua {

    /// <summary>
    /// Class for representing a Lua string value. Concrete implementation of <see cref="LuaValue"/>.
    /// </summary>
    public class LuaString : LuaValue {

        private string m_internalStringValue;

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
        public LuaValue Num() {
            if (double.TryParse(this.m_internalStringValue, out double n)) {
                return new LuaNumber(n);
            } else {
                return new LuaNil();
            }
        }

        public override bool Equals(LuaValue value) {
            if (value is LuaString s) {
                return s.m_internalStringValue.CompareTo(this.m_internalStringValue) == 0;
            } else {
                return false;
            }
        }

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override string Str() => this.m_internalStringValue;

        public override int GetHashCode() => this.m_internalStringValue.GetHashCode();

        public override string ToString() => this.m_internalStringValue;
        
        public override LuaType GetLuaType() => LuaType.LUA_STRING;

    }

}
