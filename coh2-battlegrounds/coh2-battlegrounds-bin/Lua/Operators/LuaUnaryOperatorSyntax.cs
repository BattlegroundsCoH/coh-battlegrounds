using System;
using System.Collections.Generic;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Operators {
    
    /// <summary>
    /// The expected position of the operator symbol a Lua unary operation.
    /// </summary>
    public enum LuaUnaryPosition {
        Prefix,
        Postfix,
    }

    /// <summary>
    /// Lua operator syntax handler for unary operations.
    /// </summary>
    public class LuaUnaryOperatorSyntax : ILuaOperatorSyntax {

        public string OperatorSymbol { get; }

        /// <summary>
        /// Get the expected position of the operator symbol.
        /// </summary>
        public LuaUnaryPosition Position { get; }

        public LuaUnaryOperatorSyntax(string symbol, LuaUnaryPosition position) {
            this.OperatorSymbol = symbol;
            this.Position = position;
        }

        public bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if ((i - 1 >= 0 && luaExprs[i-1] is LuaOpExpr) || i == 0) {
                luaExprs[i] = new LuaUnaryExpr(luaExprs[i + 1], this.OperatorSymbol);
                luaExprs.RemoveAt(i + 1);
                return true;
            }
            return false;
        }

        public bool PrePostCondtion(bool pre, bool post) 
            => (this.Position == LuaUnaryPosition.Prefix && post) || (this.Position == LuaUnaryPosition.Postfix && pre);

        public bool IsOperator(LuaExpr source) => source is LuaOpExpr op && op.Type is string s && s == this.OperatorSymbol;

    }

}
