using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua.Debugging;
using LuaStack = System.Collections.Generic.Stack<Battlegrounds.Lua.LuaValue>;

namespace Battlegrounds.Lua.Runtime {

    /// <summary>
    /// Base lua library functionality
    /// </summary>
    public static class LuaBaseLib {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        public static void Print(LuaState state, LuaStack stack) {
            LuaValue top = stack.Pop();
            state.Out?.WriteLine(top.ToString());
            if (state.EnableTrace) {
                Trace.WriteLine(top.Str(), "Lua");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        public static void Error(LuaState state, LuaStack stack) {
            if (stack.Count > 0) {
                throw new LuaRuntimeError(stack.Pop().Str());
            }
            throw new LuaRuntimeError($"");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int Next(LuaState state, LuaStack stack) {
            LuaValue current = stack.Pop();
            if (stack.Pop() is LuaTable t) {
                var next = t.Next(current, out LuaValue i);
                stack.Push(next);
                stack.Push(i);
            } else {
                throw new LuaRuntimeError();
            }
            return 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int Iter(LuaState state, LuaStack stack) {
            if (stack.Pop() is LuaNumber i && stack.Pop() is LuaTable t) {
                i = new LuaNumber(i + 1);
                var v = t[i];
                if (v is not LuaNil) {
                    stack.Push(i);
                    stack.Push(v);
                    return 2;
                } else {
                    stack.Push(v);
                    return 1;
                }
            } else {
                throw new LuaRuntimeError();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int IPairs(LuaState state, LuaStack stack) {
            if (stack.Pop() is LuaTable t) {
                stack.Push(new LuaClosure(new LuaFunction(Iter)));
                stack.Push(t);
                stack.Push(new LuaNumber(0));
            } else {
                throw new LuaRuntimeError();
            }
            return 3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int Pairs(LuaState state, LuaStack stack) {
            if (stack.Pop() is LuaTable t) {
                stack.Push(new LuaNil());
                stack.Push(t);
                stack.Push(state._G["next"]);
            } else {
                throw new LuaRuntimeError();
            }
            return 3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int ToString(LuaState state, LuaStack stack) {
            stack.Push(new LuaString(stack.Pop().Str()));
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int ToNumber(LuaState state, LuaStack stack) {
            var top = stack.Pop();
            if (top.GetLuaType() == LuaType.LUA_STRING && double.TryParse(top.Str(), out double d)) {
                stack.Push(new LuaNumber(d));
            } else if (top is LuaNumber) {
                stack.Push(top);
            } else {
                stack.Push(new LuaNil());
            }
            return 1;
        }

        /// <summary>
        /// Registers basic functionality in the <see cref="LuaState"/>.
        /// </summary>
        /// <param name="lState">The <see cref="LuaState"/> to import functionality into</param>
        public static void ImportLuaBase(LuaState lState) {
            
            // Register basic functions
            lState.RegisterFunction("print", Print);
            lState.RegisterFunction("error", Error);

            // Register conversion functions
            lState.RegisterFunction("tostring", ToString);
            lState.RegisterFunction("tonumber", ToNumber);

            // Register iteration functions
            lState.RegisterFunction("next", Next);
            lState.RegisterFunction("ipairs", IPairs);
            lState.RegisterFunction("pairs", Pairs);

            // Register do/load functions

        }

    }

}
