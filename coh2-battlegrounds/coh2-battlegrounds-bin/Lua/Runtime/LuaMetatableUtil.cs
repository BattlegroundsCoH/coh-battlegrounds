using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Lua.Runtime {
    
    /// <summary>
    /// Utility class for handling metatables.
    /// </summary>
    public static class LuaMetatableUtil {

        /// <summary>
        /// Handles __tostring metatable functionality.
        /// </summary>
        /// <param name="source">The source table.</param>
        /// <returns>Either the managed memory location of the table or the result of the __tostring metatable operation.</returns>
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Lua Standard")]
        public static string __ToString(LuaTable source) {
            if (source.MetaTable is LuaTable meta && meta["__tostring"] is LuaValue value) {
                if (value is LuaClosure closure) {
                    var tmpStack = new LuaStack();
                    _ = closure.Invoke(source.LuaState, tmpStack);
                    return tmpStack.PopOrNil().Str();
                } else if (value is not LuaNil) {
                    return value.Str();
                }
            }
            return $"0x{source.GetHashCode():X8}";
        }

    }

}
