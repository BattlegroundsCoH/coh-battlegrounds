namespace Battlegrounds.Lua {

    public class LuaString : LuaValue {

        private string m_internalStringValue;

        public int Length => this.m_internalStringValue.Length;

        public LuaString(string value) => this.m_internalStringValue = value;

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
