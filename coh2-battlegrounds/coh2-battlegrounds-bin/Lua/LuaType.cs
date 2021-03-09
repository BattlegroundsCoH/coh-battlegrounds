namespace Battlegrounds.Lua {
    
    public readonly struct LuaType {

        public static readonly LuaType LUA_TABLE = new LuaType("table");
        public static readonly LuaType LUA_STRING = new LuaType("string");
        public static readonly LuaType LUA_NUMBER = new LuaType("number");
        public static readonly LuaType LUA_BOOL = new LuaType("boolean");
        public static readonly LuaType LUA_NIL = new LuaType("nil");
        public static readonly LuaType LUA_USEROBJECT = new LuaType("userobject");

        public readonly string LuaTypeName;
        public LuaType(string ls) => this.LuaTypeName = ls;
        public override string ToString() => this.LuaTypeName;
    }

}
