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
            if (this.Function.IsCFunction) {
                return this.Function.Delegate.Invoke(callState, stack);
            } else {

                // Clone current environment
                var env = callState.Envionment.Clone();

                // TODO: Set captured variables (always first)

                // Set parameter values                
                int i = this.Function.Parameters.Length - 1;
                while (i >= 0) {
                    callState.Envionment[this.Function.Parameters[i]] = stack.Count > 0 ? stack.Pop() : new LuaNil();
                    i--;
                }

                // Invoke expression
                LuaVM.DoExpression(callState, this.Function.First, new Stack<LuaValue>());

                // Reset environment
                callState.Envionment = env;

                return 0; // TODO: FIX
            }
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
