namespace Battlegrounds.Lua {

    /// <summary>
    /// Represents a nil value in the Lua runtime.
    /// </summary>
    public sealed class LuaNil : LuaValue {

        /// <summary>
        /// Represents the <see cref="LuaNil"/> value. This is a read-only field.
        /// </summary>
        public static readonly LuaNil Nil = new LuaNil();

        /// <summary>
        /// Privator constructor for instances of the <see cref="LuaNil"/> class.
        /// </summary>
        private LuaNil() {}

        public override bool Equals(LuaValue value) => value is LuaNil;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override int GetHashCode() => 0;
        
        public override LuaType GetLuaType() => LuaType.LUA_NIL;

        public override string Str() => "nil";

        public override string ToString() => this.Str();

    }

}
