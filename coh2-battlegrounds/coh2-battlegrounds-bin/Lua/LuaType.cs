namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Lua value type.
    /// </summary>
    public readonly struct LuaType {

        /// <summary>
        /// Lua table type
        /// </summary>
        public static readonly LuaType LUA_TABLE = new LuaType("table");

        /// <summary>
        /// Lua string type
        /// </summary>
        public static readonly LuaType LUA_STRING = new LuaType("string");

        /// <summary>
        /// Lua number type
        /// </summary>
        public static readonly LuaType LUA_NUMBER = new LuaType("number");

        /// <summary>
        /// Lua boolean type
        /// </summary>
        public static readonly LuaType LUA_BOOL = new LuaType("boolean");

        /// <summary>
        /// Lua nil type
        /// </summary>
        public static readonly LuaType LUA_NIL = new LuaType("nil");

        /// <summary>
        /// Lua userobject type
        /// </summary>
        public static readonly LuaType LUA_USEROBJECT = new LuaType("userobject");

        /// <summary>
        /// Lua function type
        /// </summary>
        public static readonly LuaType LUA_FUNCTION = new LuaType("function");

        /// <summary>
        /// The <see cref="string"/> name of the type.
        /// </summary>
        public readonly string LuaTypeName;
        
        private LuaType(string ls) => this.LuaTypeName = ls;

        /// <summary>
        /// Determine which of two lua values that do not allow arithmetic applied.
        /// </summary>
        /// <remarks>
        /// Assumes <paramref name="right"/> is not a numeric value if <paramref name="left"/> is <see cref="LUA_NUMBER"/>.
        /// </remarks>
        /// <param name="left">The left-hand side value.</param>
        /// <param name="right">The right-hand side value.</param>
        /// <returns>If <paramref name="left"/> type is <see cref="LUA_NUMBER"/> then <paramref name="right"/> is returned; Otherwise <paramref name="left"/> is returned.</returns>
        public static LuaValue NoArithmetic(LuaValue left, LuaValue right) => (left.GetLuaType() == LUA_NUMBER) ? right : left;

        /// <summary>
        /// Determines whether to types values are representing the same type
        /// </summary>
        /// <param name="left">Left type</param>
        /// <param name="right">Right type</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is the same as <paramref name="right"/>; Otherwise <see langword="false"/>.</returns>
        public static bool operator ==(LuaType left, LuaType right)
            => left.LuaTypeName == right.LuaTypeName;

        /// <summary>
        /// Determines whether to types values are representing different types
        /// </summary>
        /// <param name="left">Left type</param>
        /// <param name="right">Right type</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is not the same as <paramref name="right"/>; Otherwise <see langword="false"/>.</returns>
        public static bool operator !=(LuaType left, LuaType right)
            => left.LuaTypeName != right.LuaTypeName;

        public override string ToString() => this.LuaTypeName;

        public override bool Equals(object obj) {
            if (obj is LuaType lt) {
                return this == lt;
            } else {
                return false;
            }
        }

        public override int GetHashCode() => base.GetHashCode();

    }

}
