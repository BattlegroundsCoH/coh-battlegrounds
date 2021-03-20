using System;
using System.Collections.Generic;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Operators {

    /// <summary>
    /// Lua operator syntax handler for binary operations.
    /// </summary>
    public class LuaBinaryOperatorSyntax : ILuaOperatorSyntax {

        public string OperatorSymbol { get; }

        public LuaBinaryOperatorSyntax(string op) {
            this.OperatorSymbol = op;
        }

        public bool PrePostCondtion(bool pre, bool post) => pre && post;

        public virtual bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if (luaExprs[i+1] is LuaFuncExpr func) {
                recursiveFunction(func.Arguments.Arguments);
                recursiveFunction(func.Body.ScopeBody);
            }
            luaExprs[i - 1] = new LuaBinaryExpr(luaExprs[i - 1], luaExprs[i + 1], this.OperatorSymbol);
            luaExprs.RemoveRange(i, 2);
            return true;
        }

        public virtual bool IsOperator(LuaExpr source) => source is LuaOpExpr op && op.Type is string s && s == this.OperatorSymbol;

    }

    /// <summary>
    /// Lua operator syntax handler for assignment operations.
    /// </summary>
    public class LuaAssignOperatorSyntax : LuaBinaryOperatorSyntax {
        
        public LuaAssignOperatorSyntax() : base("=") {}

        public override bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if (base.Apply(luaExprs, i, recursiveFunction)) {
                var binop = luaExprs[i - 1] as LuaBinaryExpr;
                luaExprs[i - 1] = new LuaAssignExpr(binop.Left, binop.Right, false);
                return true;
            } else {
                return false;
            }
        }
        
        public override bool IsOperator(LuaExpr source) => source is LuaOpExpr { Type: LuaParser.LuaTokenType.Equals };

    }

    /// <summary>
    /// Lua operator syntax handler for lookup operations.
    /// </summary>
    public class LuaLookupOperatorSyntax : LuaBinaryOperatorSyntax {
    
        public LuaLookupOperatorSyntax(string symbol) : base(symbol) { }

        public override bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if (i + 1 < luaExprs.Count && luaExprs[i + 1] is LuaLookupIdExpr or LuaLookupExpr) {
                if (base.Apply(luaExprs, i, recursiveFunction)) {
                    var binop = luaExprs[i - 1] as LuaBinaryExpr;
                    luaExprs[i - 1] = new LuaLookupExpr(binop.Left, binop.Right as LuaLookupIdExpr);
                    return true;
                }
            }
            return false;
        }

        public override bool IsOperator(LuaExpr source) => source is LuaOpExpr { Type: LuaParser.LuaTokenType.Look };

    }

    /// <summary>
    /// Lua operator syntax handler for logic operations.
    /// </summary>
    public class LuaLogicOperatorSyntax : LuaBinaryOperatorSyntax {

        public LuaLogicOperatorSyntax(string symbol) : base(symbol) { }

        public override bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
            if (base.Apply(luaExprs, i, recursiveFunction)) {
                var binop = luaExprs[i - 1] as LuaBinaryExpr;
                luaExprs[i - 1] = new LuaLogicExpr(binop.Left, binop.Right, this.OperatorSymbol);
                return true;
            }
            return false;
        }

    }
}
