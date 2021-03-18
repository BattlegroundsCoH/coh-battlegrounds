using System.Collections.Generic;

namespace Battlegrounds.Lua.Parsing {
    
    /// <summary>
    /// Abstract representation of a Lua Expression
    /// </summary>
    public abstract record LuaExpr;

    /// <summary>
    /// Abstract extension representation of a lua expression intended for looking up data
    /// </summary>
    public abstract record LuaLookupIdExpr : LuaExpr;

    /// <summary>
    /// Abtract representation of a statement.
    /// </summary>
    public abstract record LuaStatement : LuaExpr;

    /// <summary>
    /// Abstract representation of a value.
    /// </summary>
    public abstract record LuaValueExpr : LuaExpr;

    /// <summary>
    /// Nop expression (For optional executables)
    /// </summary>
    public record LuaNopExpr : LuaExpr;

    /// <summary>
    /// Represents a comment in Lua code
    /// </summary>
    public record LuaComment(string Comment) : LuaExpr;

    /// <summary>
    /// Represents a chunk of Lua expressions and statements.
    /// </summary>
    public record LuaChunk(List<LuaExpr> ScopeBody) : LuaExpr;

    /// <summary>
    /// Representation of an Operator expression (Should never be executed)
    /// </summary>
    /// <param name="Type">The operator type of the operator expression.</param>
    public record LuaOpExpr(object Type) : LuaExpr;

    /// <summary>
    /// Representation of a keyword (Should never be executed)
    /// </summary>
    public record LuaKeyword(string Keyword) : LuaExpr;

    /// <summary>
    /// Binary Lua expression with an operator defined.
    /// </summary>
    public record LuaBinaryExpr(LuaExpr Left, LuaExpr Right, string Operator) : LuaExpr;

    /// <summary>
    /// Assignment expression (Extension of <see cref="LuaBinaryExpr"/>).
    /// </summary>
    public record LuaAssignExpr(LuaExpr Left, LuaExpr Right, bool Local) : LuaBinaryExpr(Left, Right, "=");

    /// <summary>
    /// Unary expression
    /// </summary>
    public record LuaUnaryExpr(LuaExpr Expr, string Operator) : LuaExpr;

    /// <summary>
    /// Tuple expression (List of expressions) - Not an actual tuple.
    /// </summary>
    public record LuaTupleExpr(List<LuaExpr> Values) : LuaExpr;

    /// <summary>
    /// Value expression.
    /// </summary>
    public record LuaConstValueExpr(LuaValue Value) : LuaValueExpr;

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

    /// <summary>
    /// Argument list.
    /// </summary>
    public record LuaArguments(List<LuaExpr> Arguments) : LuaExpr;

    /// <summary>
    /// () group with no contents.
    /// </summary>
    public record LuaEmptyParenthesisGroup() : LuaArguments(new List<LuaExpr>());

    /// <summary>
    /// () group with a single element
    /// </summary>
    public record LuaSingleElementParenthesisGroup(List<LuaExpr> Exprs) : LuaArguments(Exprs);

    /// <summary>
    /// Call epxression
    /// </summary>
    public record LuaCallExpr(LuaExpr ToCall, LuaArguments Arguments) : LuaExpr;

    /// <summary>
    /// Functional value expression (function(...) ... end)
    /// </summary>
    public record LuaFuncExpr(LuaArguments Arguments, LuaChunk Body) : LuaValueExpr;

    /// <summary>
    /// Return statement
    /// </summary>
    public record LuaReturnStatement(LuaExpr Value) : LuaStatement;

    /// <summary>
    /// Break statement
    /// </summary>
    public record LuaBreakStatement : LuaStatement;

    /// <summary>
    /// {while &lt;Condition&gt; do &lt;Body&gt; end} statement
    /// </summary>
    public record LuaWhileStatement(LuaExpr Condition, LuaChunk Body) : LuaStatement;

    /// <summary>
    /// {do &lt;Body&gt; end} statement
    /// </summary>
    public record LuaDoStatement(LuaChunk Body) : LuaStatement;

    /// <summary>
    /// {for v=e1, e2[, e3] do &lt;Body&gt; end} statement (where e2 = max, e3 = step and is optional)
    /// </summary>
    public record LuaNumericForStatement(LuaAssignExpr Var, LuaExpr Limit, LuaExpr Step, LuaChunk Body) : LuaStatement;

    /// <summary>
    /// {for v_1, ..., v_n in e do &lt;Body&gt; end} statement (where e = iterator function)
    /// </summary>
    public record LuaGenericForStatement(List<LuaIdentifierExpr> Vars, LuaExpr Iterator, LuaChunk Body) : LuaStatement;
    // explist is evaluated only once. Its results are an iterator function, a state, and an initial value for the first iterator variable. - Lua 5.1 manual

    /// <summary>
    /// {repeat &lt;Body&gt; until &lt;Condition&gt;}  statement
    /// </summary>
    public record LuaRepeatStatement(LuaChunk Body, LuaExpr Condition) : LuaStatement;

    /// <summary>
    /// If ... then ... end statement
    /// </summary>
    public record LuaIfStatement : LuaStatement;

    /// <summary>
    /// Elseif ... then ... end statement
    /// </summary>
    public record LuaIfElseStatement : LuaStatement;

    /// <summary>
    /// Else ... end statement
    /// </summary>
    public record LuaElseStatement : LuaStatement;

}
