using System.Collections.Generic;

namespace Battlegrounds.Lua.Parsing {
    
    /// <summary>
    /// Abstract representation of a Lua Expression
    /// </summary>
    public abstract record LuaExpr(LuaSourcePos SourcePos);

    /// <summary>
    /// Abstract extension representation of a lua expression intended for looking up data
    /// </summary>
    public abstract record LuaLookupIdExpr(LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Abtract representation of a statement.
    /// </summary>
    public abstract record LuaStatement(LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Abstract representation of a value.
    /// </summary>
    public abstract record LuaValueExpr(LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Nop expression (For optional executables)
    /// </summary>
    public record LuaNopExpr(LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Represents a comment in Lua code
    /// </summary>
    public record LuaComment(string Comment, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Represents a chunk of Lua expressions and statements.
    /// </summary>
    public record LuaChunk(List<LuaExpr> ScopeBody, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Representation of an Operator expression (Should never be executed)
    /// </summary>
    /// <param name="Type">The operator type of the operator expression.</param>
    public record LuaOpExpr(object Type, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Representation of a keyword (Should never be executed)
    /// </summary>
    public record LuaKeyword(string Keyword, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Binary Lua expression with an operator defined.
    /// </summary>
    public record LuaBinaryExpr(LuaExpr Left, LuaExpr Right, string Operator) : LuaExpr(Left.SourcePos);

    /// <summary>
    /// Assignment expression (Extension of <see cref="LuaBinaryExpr"/>).
    /// </summary>
    public record LuaAssignExpr(LuaExpr Left, LuaExpr Right, bool Local) : LuaBinaryExpr(Left, Right, "=");

    /// <summary>
    /// Binary logic Lua expression with either "and" or "or" as operator.
    /// </summary>
    public record LuaLogicExpr(LuaExpr Left, LuaExpr Right, string Operator) : LuaBinaryExpr(Left, Right, Operator);

    /// <summary>
    /// Unary expression
    /// </summary>
    public record LuaUnaryExpr(LuaExpr Expr, string Operator) : LuaExpr(Expr.SourcePos);

    /// <summary>
    /// Tuple expression (List of expressions) - Not an actual tuple.
    /// </summary>
    public record LuaTupleExpr(List<LuaExpr> Values, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Value expression.
    /// </summary>
    public record LuaConstValueExpr(LuaValue Value, LuaSourcePos SourcePos) : LuaValueExpr(SourcePos);

    /// <summary>
    /// Identifier expression.
    /// </summary>
    public record LuaIdentifierExpr(string Identifier, LuaSourcePos SourcePos) : LuaLookupIdExpr(SourcePos);

    /// <summary>
    /// Indexing expression.
    /// </summary>
    public record LuaIndexExpr(LuaExpr Key) : LuaLookupIdExpr(Key.SourcePos);

    /// <summary>
    /// Overall table expression.
    /// </summary>
    public record LuaTableExpr(List<LuaExpr> SubExpressions, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Value lookup expression
    /// </summary>
    public record LuaLookupExpr(LuaExpr Left, LuaLookupIdExpr Right) : LuaLookupIdExpr(Left.SourcePos);

    /// <summary>
    /// Argument list.
    /// </summary>
    public record LuaArguments(List<LuaExpr> Arguments, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// () group with no contents.
    /// </summary>
    public record LuaEmptyParenthesisGroup(LuaSourcePos SourcePos) : LuaArguments(new List<LuaExpr>(), SourcePos);

    /// <summary>
    /// () group with a single element
    /// </summary>
    public record LuaSingleElementParenthesisGroup(List<LuaExpr> Exprs, LuaSourcePos SourcePos) : LuaArguments(Exprs, SourcePos);

    /// <summary>
    /// Call epxression
    /// </summary>
    public record LuaCallExpr(LuaExpr ToCall, LuaArguments Arguments) : LuaExpr(ToCall.SourcePos);

    /// <summary>
    /// Call on self expression
    /// </summary>
    public record LuaSelfCallExpr(LuaExpr ToCall, LuaArguments Arguments) : LuaCallExpr(ToCall, Arguments);

    /// <summary>
    /// Functional value expression (function(...) ... end)
    /// </summary>
    public record LuaFuncExpr(LuaArguments Arguments, LuaChunk Body, LuaSourcePos SourcePos) : LuaValueExpr(SourcePos);

    /// <summary>
    /// List of variables
    /// </summary>
    public record LuaVariableList(List<LuaIdentifierExpr> Variables, LuaSourcePos SourcePos) : LuaExpr(SourcePos);

    /// <summary>
    /// Return statement
    /// </summary>
    public record LuaReturnStatement(LuaExpr Value, LuaSourcePos SourcePos) : LuaStatement(SourcePos);

    /// <summary>
    /// Break statement
    /// </summary>
    public record LuaBreakStatement(LuaSourcePos SourcePos) : LuaStatement(SourcePos);

    /// <summary>
    /// {while &lt;Condition&gt; do &lt;Body&gt; end} statement
    /// </summary>
    public record LuaWhileStatement(LuaExpr Condition, LuaChunk Body) : LuaStatement(Condition.SourcePos);

    /// <summary>
    /// {do &lt;Body&gt; end} statement
    /// </summary>
    public record LuaDoStatement(LuaChunk Body, LuaSourcePos SourcePos) : LuaStatement(SourcePos);

    /// <summary>
    /// {for v=e1, e2[, e3] do &lt;Body&gt; end} statement (where e2 = max, e3 = step and is optional)
    /// </summary>
    public record LuaNumericForStatement(LuaBinaryExpr Var, LuaExpr Limit, LuaExpr Step, LuaChunk Body) : LuaStatement(Var.SourcePos);

    /// <summary>
    /// {for v_1, ..., v_n in e do &lt;Body&gt; end} statement (where e = iterator function)
    /// </summary>
    public record LuaGenericForStatement(LuaVariableList VarList, LuaExpr Iterator, LuaChunk Body) : LuaStatement(VarList.SourcePos);
    // explist is evaluated only once. Its results are an iterator function, a state, and an initial value for the first iterator variable. - Lua 5.1 manual

    /// <summary>
    /// {repeat &lt;Body&gt; until &lt;Condition&gt;}  statement
    /// </summary>
    public record LuaRepeatStatement(LuaChunk Body, LuaExpr Condition, LuaSourcePos SourcePos) : LuaStatement(SourcePos);

    /// <summary>
    /// Abstract bracnh statement
    /// </summary>
    public abstract record LuaBranch(LuaChunk Body, LuaSourcePos SourcePos) : LuaStatement(SourcePos);

    /// <summary>
    /// Abstract follow-up branch to a <see cref="LuaBranch"/>.
    /// </summary>
    public abstract record LuaBranchFollow(LuaChunk Body, LuaSourcePos SourcePos) : LuaBranch(Body, SourcePos);

    /// <summary>
    /// If ... then ... end statement
    /// </summary>
    public record LuaIfStatement(LuaExpr Condition, LuaChunk Body, LuaBranchFollow BranchFollow, LuaSourcePos SourcePos) : LuaBranch(Body, SourcePos);

    /// <summary>
    /// Elseif ... then ... end statement
    /// </summary>
    public record LuaIfElseStatement(LuaExpr Condition, LuaChunk Body, LuaBranchFollow BranchFollow, LuaSourcePos SourcePos) : LuaBranchFollow(Body, SourcePos);

    /// <summary>
    /// Else ... end statement
    /// </summary>
    public record LuaElseStatement(LuaChunk Body, LuaSourcePos SourcePos) : LuaBranchFollow(Body, SourcePos);

    /// <summary>
    /// End of branch
    /// </summary>
    public record LuaEndBranch(LuaSourcePos SourcePos) : LuaBranchFollow(new LuaChunk(new List<LuaExpr>(), SourcePos), SourcePos);

}
