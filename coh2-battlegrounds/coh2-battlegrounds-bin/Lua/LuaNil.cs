using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Lua {

    public sealed class LuaNil : LuaValue {

        public override bool Equals(LuaValue value) => value is LuaNil;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override int GetHashCode() => 0;

        public override string Str() => "nil";
        
    }

}
