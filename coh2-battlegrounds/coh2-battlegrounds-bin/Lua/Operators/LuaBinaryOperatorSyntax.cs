using System;
using System.Collections.Generic;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Operators {
    
    public class LuaBinaryOperatorSyntax : ILuaOperatorSyntax {

        public string OperatorSymbol { get; }

        public LuaBinaryOperatorSyntax(string op) {
            this.OperatorSymbol = op;
        }

        public bool PrePostCondtion(bool pre, bool post) => pre && post;

        public bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            luaExprs[i - 1] = new LuaBinaryExpr(luaExprs[i - 1], luaExprs[i + 1], this.OperatorSymbol);
            luaExprs.RemoveRange(i, 2);
            return true;
        }

    }

}
