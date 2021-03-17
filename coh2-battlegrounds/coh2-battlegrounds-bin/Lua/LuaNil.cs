namespace Battlegrounds.Lua {

    /// <summary>
    /// 
    /// </summary>
    public sealed class LuaNil : LuaValue {

        public override bool Equals(LuaValue value) => value is LuaNil;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override int GetHashCode() => 0;
        
        public override LuaType GetLuaType() => LuaType.LUA_NIL;

        public override string Str() => "nil";

        public override string ToString() => this.Str();

    }

}
