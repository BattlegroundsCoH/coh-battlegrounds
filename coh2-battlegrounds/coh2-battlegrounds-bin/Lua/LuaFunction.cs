using System.Collections.Generic;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua {

    /// <summary>
    /// Delegate for an instance of the <see cref="LuaFunction"/> class to use when invoked.
    /// </summary>
    /// <param name="state">The currently executing lua state.</param>
    /// <param name="stack">The current stack.</param>
    /// <returns>The amount of values returned by the value.</returns>
    public delegate int LuaCSharpFuncDelegate(LuaState state, Stack<LuaValue> stack); // TODO: Change to LuaClosure

    /// <summary>
    /// Simple container object for keeping track of lua function data.
    /// </summary>
    public class LuaFunction {

        private string[] m_params;
        private LuaCSharpFuncDelegate m_csharpDelegate;
        private LuaExpr m_luaTop;

        /// <summary>
        /// Get the C# delegate to invoke.
        /// </summary>
        public LuaCSharpFuncDelegate Delegate => this.m_csharpDelegate;

        /// <summary>
        /// Get the top expression to invoke in the function.
        /// </summary>
        public LuaExpr First => this.m_luaTop;

        /// <summary>
        /// Get the parameters.
        /// </summary>
        public string[] Parameters => this.m_params;

        /// <summary>
        /// Get if the <see cref="LuaFunction"/> is a C(#) function.
        /// </summary>
        public bool IsCFunction => this.m_csharpDelegate is not null;

        /// <summary>
        /// Initialize a new <see cref="LuaFunction"/> class as a native C(#) function.
        /// </summary>
        /// <param name="funcDelegate">The delegate to execute when a <see cref="LuaClosure"/> is being invoked.</param>
        public LuaFunction(LuaCSharpFuncDelegate funcDelegate) {
            this.m_csharpDelegate = funcDelegate;
            this.m_params = null;
            this.m_luaTop = null;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaFunction"/> class with lua AST data and parameter data.
        /// </summary>
        /// <param name="raw">The raw AST code data to execute when a <see cref="LuaClosure"/> is being invoked.</param>
        /// <param name="parameters">The name of the parameters to map stack values to.</param>
        public LuaFunction(LuaExpr raw, params string[] parameters) {
            this.m_csharpDelegate = null;
            this.m_luaTop = raw;
            this.m_params = parameters;
        }

        public override int GetHashCode() => this.IsCFunction ? this.m_csharpDelegate.GetHashCode() : this.m_luaTop.GetHashCode();

    }

}
