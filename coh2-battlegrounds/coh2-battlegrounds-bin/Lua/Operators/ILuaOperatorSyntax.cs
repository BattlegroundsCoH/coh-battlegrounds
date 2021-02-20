using System;
using System.Collections.Generic;

namespace Battlegrounds.Lua.Operators {
    
    public interface ILuaOperatorSyntax {

        string OperatorSymbol { get; }

        bool PrePostCondtion(bool pre, bool post);

        bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction);

    }

}
