using System;
using System.Collections.Generic;

using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Operators {

    /// <summary>
    /// Lua operator syntax handler for indexing operations.
    /// </summary>
    public class LuaIndexOperatorSyntax : ILuaOperatorSyntax {

        public string OperatorSymbol => "[]";

        public bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if (i - 1 >= 0 && luaExprs[i - 1] is LuaLookupIdExpr) {

                // Set args
                var largs = luaExprs[i] as LuaIndexExpr;
                luaExprs[i - 1] = new LuaLookupExpr(luaExprs[i - 1], largs);
                luaExprs.RemoveAt(i);

                // Recursively apply OOP on arguments
                if (largs.Key is LuaChunk) {
                    // TODO: Parse
                }

                return true;
            } else {
                return false;
            }
        }

        public bool IsOperator(LuaExpr source) => source is LuaIndexExpr;

        public bool PrePostCondtion(bool pre, bool post) => pre is true;

    }

}
