using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Lua {

    /// <summary>
    /// Representation of a Lua table. Extends <see cref="LuaValue"/> and can be enumerated over.
    /// </summary>
    public sealed class LuaTable : LuaValue, IEnumerable<KeyValuePair<LuaValue, LuaValue>> {

        private Dictionary<LuaValue, LuaValue> m_table;

        /// <summary>
        /// Initialize a new <see cref="LuaTable"/> class without entries.
        /// </summary>
        public LuaTable() {
            this.m_table = new Dictionary<LuaValue, LuaValue>();
        }

        public override bool Equals(LuaValue value) {
            if (value is LuaTable table) {
                return this.m_table.All(x => table.Contains(x));
            }
            return false;
        }

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public LuaValue this[LuaValue key] {
            get => this.m_table[key];
            set => this.m_table[key] = value;
        }

        public LuaValue this[string key] {
            get => this.m_table[new LuaString(key)];
            set => this.m_table[new LuaString(key)] = value;
        }

        public T ByKey<T>(LuaValue key) where T : LuaValue {
            if (this.m_table[key] is T v)
                return v;
            else
                return default;
        }

        public T ByKey<T>(string key) where T : LuaValue {
            if (this.m_table[new LuaString(key)] is T v)
                return v;
            else
                return default;
        }

        public override string Str() => this.GetHashCode().ToString();

        public IEnumerator<KeyValuePair<LuaValue, LuaValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<LuaValue, LuaValue>>)this.m_table).GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_table).GetEnumerator();

        public override int GetHashCode() => this.m_table.GetHashCode();
    
    }

}
