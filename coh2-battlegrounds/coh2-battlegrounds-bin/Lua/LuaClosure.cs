using Battlegrounds.Lua.Runtime;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Closure value type for Lua. Extends <see cref="LuaValue"/>.
    /// </summary>
    public class LuaClosure : LuaValue {

        /// <summary>
        /// Get the <see cref="LuaFunction"/> to execute when closure is invoked.
        /// </summary>
        public LuaFunction Function { get; }

        /// <summary>
        /// Initialize a new <see cref="LuaClosure"/> class with specified <see cref="LuaFunction"/>.
        /// </summary>
        /// <param name="function">The function to execute when closure is invoked.</param>
        public LuaClosure(LuaFunction function) {
            this.Function = function;
        }

        /// <summary>
        /// Invoke the referenced <see cref="LuaFunction"/> within the <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="callState">The <see cref="LuaState"/> responsible for invoking the closure.</param>
        /// <param name="stack">The current stack containing <see cref="LuaFunction"/> values.</param>
        /// <returns>The amount of values returned by the stack.</returns>
        public int Invoke(LuaState callState, LuaStack stack) {

            if (this.Function.IsCFunction) {
                return this.Function.Delegate.Invoke(callState, stack);
            } else {

                // Clone current environment
                callState.Envionment.NewFrame();

                // TODO: Set captured variables (always first)

                // Set parameter values                
                int i = this.Function.Parameters.Length - 1;
                while (i >= 0) {
                    callState.Envionment.Define(this.Function.Parameters[i], stack.PopOrNil());
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
                callState.Envionment.PopFrame();

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

        public override int GetHashCode() => this.Function.GetHashCode();

        public override LuaType GetLuaType() => LuaType.LUA_FUNCTION;

        public override string Str() => $"0x:{this.GetHashCode():X8}";

        public override string ToString() => this.Str();

    }

}
