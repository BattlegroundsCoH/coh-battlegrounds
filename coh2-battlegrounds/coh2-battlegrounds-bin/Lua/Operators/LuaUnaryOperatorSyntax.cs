using System;
using System.Collections.Generic;

namespace Battlegrounds.Lua.Operators {
    
    public enum LuaUnaryPosition {
        Prefix,
        Postfix,
    }

    public class LuaUnaryOperatorSyntax : ILuaOperatorSyntax {

        public string OperatorSymbol { get; }

        public LuaUnaryPosition Position { get; }

        public LuaUnaryOperatorSyntax(string symbol, LuaUnaryPosition position) {
            this.OperatorSymbol = symbol;
            this.Position = position;
        }

        public bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if ((i - 1 >= 0 && luaExprs[i-1] is LuaOpExpr) || i == 0) {
                luaExprs[i] = new LuaNegateExpr(luaExprs[i + 1]);
                luaExprs.RemoveAt(i + 1);
                return true;
            }
            return false;
        }

        public bool PrePostCondtion(bool pre, bool post) 
            => (this.Position == LuaUnaryPosition.Prefix && post) || (this.Position == LuaUnaryPosition.Postfix && pre);

    }

}
