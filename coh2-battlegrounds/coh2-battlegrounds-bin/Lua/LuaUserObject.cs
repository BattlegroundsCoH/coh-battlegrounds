using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Lua {

    public class LuaUserObject : LuaValue {

        private object m_obj;

        public LuaUserObject(object o) => this.m_obj = o;

        public override bool Equals(LuaValue value) => value is LuaUserObject o && o.m_obj == this.m_obj;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override int GetHashCode() => this.m_obj.GetHashCode();
        
        public override string Str() => this.m_obj.ToString();

    }

}
