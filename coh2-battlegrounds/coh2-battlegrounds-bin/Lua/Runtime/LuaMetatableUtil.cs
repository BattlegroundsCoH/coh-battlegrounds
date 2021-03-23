using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Lua.Runtime {
    
    /// <summary>
    /// Utility class for handling metatables.
    /// </summary>
    public static class LuaMetatableUtil {

        private static LuaValue GetOrInvoke(LuaValue value, LuaState state, params LuaValue[] args) {
            if (value is LuaClosure closure) {
                var tmpStack = new LuaStack();
                for (int i = 0; i < args.Length; i++) {
                    tmpStack.Push(args[i]);
                }
                _ = closure.Invoke(state, tmpStack);
                return tmpStack.PopOrNil();
            } else {
                return value;
            }
        }

        /// <summary>
        /// Handles __tostring metatable functionality.
        /// </summary>
        /// <param name="source">The source table.</param>
        /// <returns>Either the managed memory location of the table or the result of the __tostring metatable operation.</returns>
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Lua Standard")]
        public static string __ToString(LuaTable source) { // In this case it should always be a table, as ToString calls can be handled easily for user objects
            if (source.MetaTable is LuaTable meta && meta["__tostring"] is LuaValue value) {
                if (GetOrInvoke(value, source.LuaState, source) is LuaValue v and not LuaNil) {
                    return v.Str();
                }
            }
            return $"0x{source.GetHashCode():X8}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Lua Standard")]
        public static LuaValue __Index(IMetatableParent source, LuaValue key, LuaState state) {
            if (source is LuaTable table && table[key] is LuaValue value and not LuaNil) {
                return value;
            } else if (source.MetaTable is not null) {
                if (GetOrInvoke(source.MetaTable["__index"], state, source as LuaValue, key) is LuaValue v) {
                    return v;
                }
            }
            return new LuaNil();
        }

    }

}
