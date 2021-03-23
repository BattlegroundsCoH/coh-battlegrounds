namespace Battlegrounds.Lua {

    /// <summary>
    /// Represents a C (C#) object exposed to Lua. (In this case it's a managed C# object, this also means it'll offer some extra functionality).
    /// </summary>
    public class LuaUserObject : LuaValue {

        private object m_obj;

        /// <summary>
        /// 
        /// </summary>
        public object Object => this.m_obj;

        /// <summary>
        /// Get or privately set the metatable of the userobject.
        /// </summary>
        public LuaValue Meta { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        public LuaUserObject(object o) => this.m_obj = o;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>() => (T)this.m_obj;

        public override bool Equals(LuaValue value) => value is LuaUserObject o && o.m_obj == this.m_obj;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override int GetHashCode() => this.m_obj.GetHashCode();
        
        public override string Str() => $"0x{this.m_obj.GetHashCode():X8}";

        public override LuaType GetLuaType() => LuaType.LUA_USEROBJECT;

        public override string ToString() => this.m_obj.ToString();

    }

}
