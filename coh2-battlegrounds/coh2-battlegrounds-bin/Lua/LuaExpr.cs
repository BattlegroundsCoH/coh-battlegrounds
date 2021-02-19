using System.Collections.Generic;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Abstract representation of a Lua Expression
    /// </summary>
    public abstract record LuaExpr();

    /// <summary>
    /// Abstract extension representation of a lua expression intended for looking up data
    /// </summary>
    public abstract record LuaLookupIdExpr() : LuaExpr;

    /// <summary>
    /// Represents a comment in Lua code
    /// </summary>
    public record LuaComment(string Comment) : LuaExpr;

    /// <summary>
    /// Representation of an Operator expression (Should never be executed)
    /// </summary>
    /// <param name="Type">The operator type of the operator expression.</param>
    public record LuaOpExpr(object Type) : LuaExpr;

    /// <summary>
    /// Binary Lua expression with an operator defined.
    /// </summary>
    public record LuaBinaryExpr(LuaExpr Left, LuaExpr Right, string Operator) : LuaExpr;

    /// <summary>
    /// Value expression.
    /// </summary>
    public record LuaValueExpr(LuaValue Value) : LuaExpr;

    /// <summary>
    /// Identifier expression.
    /// </summary>
    public record LuaIdentifierExpr(string Identifier) : LuaLookupIdExpr;

    /// <summary>
    /// Indexing expression.
    /// </summary>
    public record LuaIndexExpr(LuaExpr Key) : LuaLookupIdExpr;

    /// <summary>
    /// Overall table expression.
    /// </summary>
    public record LuaTableExpr(List<LuaExpr> SubExpressions) : LuaExpr;

    /// <summary>
    /// Value lookup expression
    /// </summary>
    public record LuaLookupExpr(LuaExpr Left, LuaLookupIdExpr Right) : LuaExpr;

}
