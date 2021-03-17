using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="stack"></param>
    /// <returns></returns>
    public delegate int LuaCSharpFuncDelegate(LuaState state, Stack<LuaValue> stack); // TODO: Change to LuaClosure

    /// <summary>
    /// 
    /// </summary>
    public class LuaFunction {

        private string[] m_params;
        private LuaCSharpFuncDelegate m_csharpDelegate;
        private LuaExpr m_luaTop;

        /// <summary>
        /// 
        /// </summary>
        public bool IsCFunction => this.m_csharpDelegate is not null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcDelegate"></param>
        public LuaFunction(LuaCSharpFuncDelegate funcDelegate) {
            this.m_csharpDelegate = funcDelegate;
            this.m_params = null;
            this.m_luaTop = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        public LuaFunction(LuaExpr raw, params string[] parameters) {
            this.m_csharpDelegate = null;
            this.m_luaTop = raw;
            this.m_params = parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callState"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public int Invoke(LuaState callState, Stack<LuaValue> stack) {
            if (this.IsCFunction) {
                return this.m_csharpDelegate.Invoke(callState, stack);
            } else {
                stack.Push(LuaVM.DoExpression(callState, this.m_luaTop, stack));
                return stack.Count; // TODO: We may have to make DoExpression return count as well and also return stack
            }
        }

    }

}
