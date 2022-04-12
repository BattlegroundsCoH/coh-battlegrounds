using System;
using System.Collections.Generic;

using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Operators {

    /// <summary>
    /// Interface for handling the syntax of Lua operators.
    /// </summary>
    public interface ILuaOperatorSyntax {

        /// <summary>
        /// The symbol(s) used to identify the operator
        /// </summary>
        string OperatorSymbol { get; }

        /// <summary>
        /// Checks if a <paramref name="source"/> is of the expected operator type.
        /// </summary>
        /// <param name="source">The operator expression expected to parse value.</param>
        /// <returns><see langword="true"/> if <paramref name="source"/> is expected operator; Otherwise <see langword="false"/>.</returns>
        bool IsOperator(LuaExpr source);

        /// <summary>
        /// Checks if, given <paramref name="pre"/> and <paramref name="post"/> conditions will lead to a valid <see cref="ILuaOperatorSyntax"/> case.
        /// </summary>
        /// <param name="pre">Is there a <see cref="LuaExpr"/> in the previous position of given stream of expressions.</param>
        /// <param name="post">Is there a <see cref="LuaExpr"/> in the following position of given stream of expressions.</param>
        /// <returns><see langword="true"/> if <paramref name="post"/> and <paramref name="pre"/> matches <see cref="ILuaOperatorSyntax"/> requirements; Otherwise <see langword="false"/>.</returns>
        bool PrePostCondtion(bool pre, bool post);

        /// <summary>
        /// Apply the operator syntax to the <paramref name="luaExprs"/> list.
        /// </summary>
        /// <param name="luaExprs">The list of expressions to apply <see cref="ILuaOperatorSyntax"/> syntax on.</param>
        /// <param name="i">The index of the <see cref="LuaExpr"/> that triggered the application process.</param>
        /// <param name="recursiveFunction">The function to recursively call on sub-elements.</param>
        /// <returns><see langword="true"/> if <see cref="ILuaOperatorSyntax"/> was applied; Otherwise <see langword="false"/>.</returns>
        bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction);

    }

}
