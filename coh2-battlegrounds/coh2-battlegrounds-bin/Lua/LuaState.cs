using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Battlegrounds.Functional;
using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Runtime;
using Battlegrounds.Lua.Parsing;
using System.Diagnostics;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Basic C# implementation of the C-type lua_State for Lua.
    /// </summary>
    public class LuaState {

        public struct CallStackFrame {
            public string FrameTitle;
            public LuaSourcePos FrameSource;
        }

        private int m_initialGSize;
        private LuaException m_lastError;
        private Stack<CallStackFrame> m_callStack;
        private Dictionary<Type, LuaUserObjectType> m_userTypes;

#pragma warning disable IDE1006 // Naming Styles (This is intentional in Lua

        /// <summary>
        /// Get the global table containing all data relevant to the <see cref="LuaState"/>.
        /// </summary>
        public LuaTable _G { get; }

        /// <summary>
        /// Get or set the current environment values (Arguments and local variables).
        /// </summary>
        public LuaEnvironment Envionment { get; set; }

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

        /// <summary>
        /// Get or set whether the <see cref="LuaVM"/> should trace stack data.
        /// </summary>
        public bool StackTrace { get; set; } = false;

        /// <summary>
        /// Get or set whether the <see cref="LuaVM"/> should execute instructions with debugging capabilities.
        /// </summary>
        public bool DebugMode { get; set; } = false;

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
            this._G = new LuaTable(this);
            this._G["_G"] = this._G; // Assign _G to self
            this._G["__version"] = new LuaString("Battlegrounds.Lua V1.0 (Emulates Lua 5.1)");

            // Initialize envionment
            this.Envionment = new LuaEnvironment();

            // Set console in and out
            this.Out = Console.Out;
            this.In = Console.In;

            // Register libraries
            libraries.Distinct().ForEach(x => {
                switch (x) {
                    case "base":
                        LuaBaseLib.ImportLuaBase(this);
                        break;
                    default:
                        Trace.WriteLine($"Invalid built-in Lua library '{x}'", "Lua");
                        break;
                }
            });

            // Store the size of the initial _G table
            this.m_initialGSize = _G.Size;

            // Init callstack
            this.m_callStack = new Stack<CallStackFrame>();

            // Init type lists
            this.m_userTypes = new Dictionary<Type, LuaUserObjectType>();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LuaUserObjectType GetUsertype(Type type) { 
            if (this.m_userTypes.TryGetValue(type, out LuaUserObjectType objType)) {
                return objType;
            } else {
                throw new LuaException($"Invalid type '{type.FullName}'; Not registered in state.");
            }
        }

        /// <summary>
        /// Get last runtime error (<see langword="null"/> if no runtime error).
        /// </summary>
        /// <returns>Returns the last thrown <see cref="LuaException"/> within the context of this state.</returns>
        public LuaException GetError() => this.m_lastError;

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
        /// Set the last <see cref="LuaException"/> that occured.
        /// </summary>
        /// <param name="luaRuntimeErr">The last <see cref="LuaException"/> instance.</param>
        public void SetLastError(LuaException luaRuntimeErr) => this.m_lastError = luaRuntimeErr;

        /// <summary>
        /// Register a function within the global environment.
        /// </summary>
        /// <param name="name">The name of the function to register.</param>
        /// <param name="sharpFuncDelegate">The <see cref="LuaCSharpFuncDelegate"/> delegate to use as function.</param>
        public void RegisterFunction(string name, LuaCSharpFuncDelegate sharpFuncDelegate) 
            => this._G[name] = new LuaClosure(new LuaFunction(sharpFuncDelegate));

        /// <summary>
        /// Register a void function within the global environment.
        /// </summary>
        /// <remarks>
        /// This will convert the given <see cref="Action{T1, T2}"/> into a <see cref="LuaCSharpFuncDelegate"/> that pushes a <see cref="LuaNil"/> value on the stack.
        /// </remarks>
        /// <param name="name">The name of the function to register.</param>
        /// <param name="action">The void <see cref="Action{T1, T2}"/> to invoke when the function is called by Lua.</param>
        public void RegisterFunction(string name, Action<LuaState, LuaStack> action)
            => this._G[name] = new LuaClosure(new LuaFunction((a,b) => {
                action(a, b);
                b.Push(new LuaNil());
                return 1;
            }));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public void RegisterUserdata(Type type) {
            
            // Collect methods and properties
            var managedMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<LuaUserobjectMethodAttribute>() is not null)
                .Select(x => new LuaUserObjectType.Method(x.Name, x.GetCustomAttribute<LuaUserobjectMethodAttribute>(), x));
            var managedProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<LuaUserobjectPropertyAttribute>() is not null && x.GetMethod is not null)
                .Select(x => new LuaUserObjectType.Property(x.Name, x.GetCustomAttribute<LuaUserobjectPropertyAttribute>(), x));
            
            // Create type
            LuaUserObjectType objType = new LuaUserObjectType(type);
            objType.Methods.AddRange(managedMethods);
            objType.Properties.AddRange(managedProperties);
            objType.CreateInstanceMetaTable(this);

            // Init table container
            LuaTable userDataTable = new LuaTable(this);

            // Init methods
            foreach (var method in managedMethods) {
                userDataTable[method.Name] = LuaUserObjectType.CreateFunction(objType, method, userDataTable);
            }

            // Create metatable
            userDataTable["__metatable"] = LuaUserObjectType.CreateMetatable(objType, this);

            // Register usertype
            this._G[type.Name] = userDataTable;
            this.m_userTypes[type] = objType;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsUserObject(Type type) => this.m_userTypes.ContainsKey(type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePos"></param>
        /// <param name="comment"></param>
        public void PushStackTrace(LuaSourcePos sourcePos, string comment) {
            CallStackFrame stackFrame = new CallStackFrame() { FrameSource = sourcePos, FrameTitle = comment };
            this.m_callStack.Push(stackFrame);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CallStackFrame PopStackTrace() => this.m_callStack.Pop();

    }

}
