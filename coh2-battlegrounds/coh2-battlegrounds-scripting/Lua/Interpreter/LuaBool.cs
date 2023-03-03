namespace Battlegrounds.Scripting.Lua.Interpreter;

public class LuaBool : LuaValue {

    private bool m_bool;

    public bool IsTrue => this.m_bool;

    public LuaBool(bool value) => this.m_bool = value;

    public override bool Equals(LuaValue value) => value is LuaBool b && b.m_bool == this.m_bool;

    public override bool Equals(object? obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

    public override string Str() => this.m_bool ? "true" : "false";

    public override int GetHashCode() => this.m_bool.GetHashCode();

    public override LuaType GetLuaType() => LuaType.LUA_BOOL;

    public override string ToString() => this.Str();

}
