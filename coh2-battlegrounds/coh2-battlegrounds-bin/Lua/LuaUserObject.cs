namespace Battlegrounds.Lua {

    /// <summary>
    /// Represents a C (C#) object exposed to Lua. (In this case it's a managed C# object, this also means it'll offer some extra functionality).
    /// </summary>
    public class LuaUserObject : LuaValue, IMetatableParent {

        private object m_obj;

        /// <summary>
        /// Get the wrapped C# object.
        /// </summary>
        public object Object => this.m_obj;

        /// <summary>
        /// Get or privately set the metatable of the userobject.
        /// </summary>
        public LuaTable MetaTable { get; private set; }

        /// <summary>
        /// Initialize a new <see cref="LuaUserObject"/> class with wrapped C# object.
        /// </summary>
        /// <param name="o">The C# object to represent.</param>
        public LuaUserObject(object o) => this.m_obj = o;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>() => (T)this.m_obj;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meta"></param>
        public void SetMetatable(LuaValue meta) {
            if (meta is LuaTable t) {
                this.MetaTable = t;
            } else {
                this.MetaTable = null;
            }
        }

        public override bool Equals(LuaValue value) => value is LuaUserObject o && o.m_obj == this.m_obj;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override int GetHashCode() => this.m_obj.GetHashCode();
        
        public override string Str() => $"0x{this.m_obj.GetHashCode():X8}";

        public override LuaType GetLuaType() => LuaType.LUA_USEROBJECT;

        public override string ToString() => this.m_obj.ToString();

    }

}
