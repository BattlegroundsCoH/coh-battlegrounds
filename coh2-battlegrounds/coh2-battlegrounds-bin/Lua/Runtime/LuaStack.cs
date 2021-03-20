using System.Collections.Generic;

namespace Battlegrounds.Lua.Runtime {
    
    /// <summary>
    /// Represents the stack of <see cref="LuaValue"/> objects.
    /// </summary>
    public class LuaStack : Stack<LuaValue> {

        /// <summary>
        /// Pops the top element of the stack. If no element is available, <see cref="LuaNil"/> is returned.
        /// </summary>
        /// <returns>The top element or <see cref="LuaNil"/> if there's no top element.</returns>
        public LuaValue PopOrNil() {
            if (this.Count > 0) {
                return this.Pop();
            } else {
                return new LuaNil();
            }
        }

    }

}
