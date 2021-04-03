using System.Linq;
using System.Collections.Generic;
using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Runtime;

namespace Battlegrounds.Lua {

    /// <summary>
    /// Utility class for marshalling between the Lua runtime and the C# runtime.
    /// </summary>
    public static class LuaMarshal {

        /// <summary>
        /// Convert a <see cref="LuaValue"/> into its closest managed equivalent.
        /// </summary>
        /// <param name="value">The <see cref="LuaValue"/> to convert.</param>
        /// <returns>The closest managed representation possible; Otherwise, if no proper representation can be found, <see langword="null"/> is returned.</returns>
        public static object FromLuaValue(LuaValue value) => value switch {
            LuaString s => s.Str(),
            LuaNumber n => n.IsInteger() ? (int)n : (double)n,
            LuaBool b => b.IsTrue,
            LuaTable t => t,
            LuaUserObject u => u.Object,
            LuaClosure c => c.Function,
            _ => null
        };

        /// <summary>
        /// Convert a managed object into a proper <see cref="LuaValue"/> representation.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to get <see cref="LuaValue"/> representation of.</param>
        /// <returns>The best <see cref="LuaValue"/> representation of <paramref name="value"/>. If no proper representation could be found a <see cref="LuaUserObject"/> is used.</returns>
        public static LuaValue ToLuaValue(object value) => value switch {
            double d => new LuaNumber(d),
            float f => new LuaNumber(f),
            int i => new LuaNumber(i),
            string s => new LuaString(s),
            bool b => new LuaBool(b),
            null => new LuaNil(),
            _ => new LuaUserObject(value)
        };

        /// <summary>
        /// Invoke a given <see cref="LuaUserObjectType.Method"/> from the Lua runtime.
        /// </summary>
        /// <param name="state">The calling state.</param>
        /// <param name="stack">The stack at call time.</param>
        /// <param name="method">The actual method to invoke.</param>
        /// <returns>Always 1, as either the method return value is marshalled or returned; Otherwise <see cref="LuaNil"/> is returned.</returns>
        public static int InvokeAsLua(LuaState state, LuaStack stack, LuaUserObjectType.Method method) {
            List<object> parameters = new List<object>();
            int expectedCount = method.Info.GetParameters().Length + (method.Info.IsStatic ? 0 : 1);
            while (parameters.Count < expectedCount) {
                parameters.Add(FromLuaValue(stack.PopOrNil()));
            }
            parameters.Reverse();
            if (method.Info.IsStatic) {
                stack.Push(ToLuaValue(method.Info.Invoke(null, parameters.ToArray())));
            } else {
                bool validSelf = parameters[0].GetType() == method.Info.DeclaringType || parameters[0].GetType().IsAssignableTo(method.Info.DeclaringType);
                if (validSelf) {
                    stack.Push(ToLuaValue(method.Info.Invoke(parameters[0], parameters.Skip(1).ToArray())));
                } else {
                    throw new LuaRuntimeError($"Attempt to invoke native method {method.Name} without valid instance.");
                }
            }
            return 1;
        }

        /// <summary>
        /// Invoke a <see cref="LuaClosure"/> directly from managed code and get result(s) as managed objects.
        /// </summary>
        /// <param name="closure">The <see cref="LuaClosure"/> to invoke (without proper environment).</param>
        /// <param name="state">The <see cref="LuaState"/> to use to invoke the closure.</param>
        /// <param name="args">The managed objects to marshal into their respective Lua representations.</param>
        /// <returns>An array of managed objects demarshalled from their respective Lua representations.</returns>
        /// <exception cref="LuaException"/>
        /// <exception cref="LuaRuntimeError"/>
        public static object[] InvokeClosureManaged(LuaClosure closure, LuaState state, params object[] args) {
            LuaStack stack = new LuaStack();
            for (int i = 0; i < args.Length; i++) {
                var luaValue = ToLuaValue(args[i]);
                if (luaValue is LuaUserObject obj) {
                    obj.SetMetatable(state.GetUsertype(obj.Type).InstanceMetatable);
                }
                stack.Push(luaValue);
            }
            int pop = closure.Invoke(state, stack);
            return stack.Pop(pop).Select(x => FromLuaValue(x)).ToArray();
        }

    }

}
