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
        /// Invoke the referenced <see cref="LuaFunction"/> within the <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="callState">The <see cref="LuaState"/> responsible for invoking the closure.</param>
        /// <param name="stack">The current stack containing <see cref="LuaFunction"/> values.</param>
        /// <returns>The amount of values returned by the stack.</returns>
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
                var _stack = LuaVM.DoExpression(callState, this.Function.First);
                int _stackSz = _stack.Count;

                // Push returned values onto calling stack
                while (_stack.Count > 0) {
                    stack.Push(_stack.Pop());
                }

                // Reset environment
                callState.Envionment = env;

                // Return stack size
                return _stackSz;
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
