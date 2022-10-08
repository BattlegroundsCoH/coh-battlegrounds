namespace Battlegrounds.Scripting.Lua.Parsing.Operators;

/// <summary>
/// Lua operator syntax handler for call operations.
/// </summary>
public class LuaCallOperatorSyntax : ILuaOperatorSyntax {

    public string OperatorSymbol => "()";

    public bool Apply(List<LuaExpr> luaExprs, int i, Action<List<LuaExpr>> recursiveFunction) {
        if (i - 1 >= 0 && luaExprs[i - 1] is LuaLookupIdExpr) {

            // Set args
            var largs = luaExprs[i] as LuaArguments;
            luaExprs[i - 1] = new LuaCallExpr(luaExprs[i - 1], largs);
            luaExprs.RemoveAt(i);

            // Recursively apply OOP on arguments
            recursiveFunction(largs.Arguments);

            return true;
        } else {
            return false;
        }
    }

    public bool IsOperator(LuaExpr source) => source is LuaArguments;

    public bool PrePostCondtion(bool pre, bool post) => pre is true;

}
