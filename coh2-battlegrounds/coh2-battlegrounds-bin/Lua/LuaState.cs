using System;
using System.Collections.Generic;
using System.IO;
using Battlegrounds.Functional;
using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Runtime;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Basic C# implementation of the C-type lua_State for Lua.
    /// </summary>
    public class LuaState {

        private int m_initialGSize;
        private LuaRuntimeError m_lastError;

#pragma warning disable IDE1006 // Naming Styles (This is intentional in Lua

        /// <summary>
        /// Get the global table containing all data relevant to the <see cref="LuaState"/>.
        /// </summary>
        public LuaTable _G { get; }

        /// <summary>
        /// Get or set the current environment values (Arguments and local variables).
        /// </summary>
        public LuaTable Envionment { get; set; }

        /// <summary>
        /// Get the output writer of the state.
        /// </summary>
        public TextWriter Out { get; set; }

        /// <summary>
        /// Get the input reader of the state
        /// </summary>
        public TextReader In { get; set; }

        /// <summary>
        /// Get or set whether <see cref="System.Diagnostics.Trace"/> should be used.
        /// </summary>
        public bool EnableTrace { get; set; } = true;

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Get the initial size of the <see cref="_G"/> table.
        /// </summary>
        public int InitialGlobalSize => this.m_initialGSize;

        /// <summary>
        /// Create new <see cref="LuaState"/> with <see cref="_G"/> initialized.
        /// </summary>
        public LuaState(params string[] libraries) {
            
            // Create _G table
            this._G = new LuaTable();
            this._G["_G"] = this._G; // Assign _G to self
            this._G["__version"] = new LuaString("Battlegrounds.Lua V1.0 (Emulates Lua 5.1)");

            // Initialize envionment
            this.Envionment = new LuaTable();

            // Set console in and out
            this.Out = Console.Out;
            this.In = Console.In;

            // Register libraries
            libraries.ForEach(x => {
                switch (x) {
                    case "base":
                        LuaBaseLib.ImportLuaBase(this);
                        break;
                    default:
                        break;
                }
            });

            // Store the size of the initial _G table
            this.m_initialGSize = _G.Size;

        }

        /// <summary>
        /// Get last runtime error (<see langword="null"/> if no runtime error).
        /// </summary>
        /// <returns>Returns the last thrown <see cref="LuaRuntimeError"/>.</returns>
        public LuaRuntimeError GetError() => this.m_lastError;

        /// <summary>
        /// Do a Lua string expression in the current <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="luaExpression">The lua-code string containing the expression(s) to do.</param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        public LuaValue DoString(string luaExpression) => LuaVM.DoString(this, luaExpression);

        /// <summary>
        /// Do a Lua file containing Lua source code in the current <see cref="LuaState"/> envionment.
        /// </summary>
        /// <param name="luaSourceFilePath"></param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        /// <exception cref="FileNotFoundException"/>
        public LuaValue DoFile(string luaSourceFilePath) => LuaVM.DoFile(this, luaSourceFilePath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaRuntimeErr"></param>
        public void SetLastError(LuaRuntimeError luaRuntimeErr) => this.m_lastError = luaRuntimeErr;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sharpFuncDelegate"></param>
        public void RegisterFunction(string name, LuaCSharpFuncDelegate sharpFuncDelegate) 
            => this._G[name] = new LuaClosure(new LuaFunction(sharpFuncDelegate));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterFunction(string name, Action<LuaState, Stack<LuaValue>> action)
            => this._G[name] = new LuaClosure(new LuaFunction((a,b) => {
                action(a, b);
                return 0;
            }));

    }

}
