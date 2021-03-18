using System;
using System.Collections.Generic;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Operators {
    
    /// <summary>
    /// 
    /// </summary>
    public interface ILuaOperatorSyntax {

        /// <summary>
        /// 
        /// </summary>
        string OperatorSymbol { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        bool IsOperator(LuaExpr source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pre"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        bool PrePostCondtion(bool pre, bool post);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaExprs"></param>
        /// <param name="i"></param>
        /// <param name="recursiveFunction"></param>
        /// <returns></returns>
        bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction);

    }

}
