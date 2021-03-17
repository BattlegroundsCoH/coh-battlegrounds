using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// 
    /// </summary>
    public class LuaClosure : LuaValue {

        /// <summary>
        /// 
        /// </summary>
        public LuaFunction Function { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        public LuaClosure(LuaFunction function) {
            this.Function = function;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callState"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public int Invoke(LuaState callState, Stack<LuaValue> stack) {
            // TODO: Set captured variables
            return this.Function.Invoke(callState, stack);
        }

        public override bool Equals(LuaValue value) {
            if (value is LuaClosure func) {
                return func == this;
            } else {
                return false;
            }
        }

        public override int GetHashCode() => 0;

        public override LuaType GetLuaType() => LuaType.LUA_FUNCTION;

        public override string Str() => $"0x:{this.GetHashCode():X8}";

    }

}
