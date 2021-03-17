using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Lua.Runtime {

    public static class LuaBaseLib {

        public static void Print(LuaState state, Stack<LuaValue> stack) {
            LuaValue top = stack.Pop();
            state.Out.WriteLine(top.ToString());
            if (state.EnableTrace) {
                Trace.WriteLine(top.Str(), "Lua");
            }
        }

        public static void ImportLuaBase(LuaState lState) {
            
            lState.RegisterFunction("print", Print);

        }

    }

}
