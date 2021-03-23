using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua.Debugging;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// 
    /// </summary>
    public class LuaUserObjectType {

        /// <summary>
        /// 
        /// </summary>
        public record Method(string Name, LuaUserobjectMethodAttribute MethodAttribute, MethodInfo Info);

        /// <summary>
        /// 
        /// </summary>
        public record Property(string Name, LuaUserobjectPropertyAttribute PropertyAttribute, PropertyInfo Info);

        /// <summary>
        /// 
        /// </summary>
        public Type ObjectType { get; }

        /// <summary>
        /// 
        /// </summary>
        public List<Method> Methods { get; }

        /// <summary>
        /// 
        /// </summary>
        public List<Property> Properties { get; }

        public LuaUserObjectType(Type type) {
            this.ObjectType = type;
            this.Methods = new List<Method>();
            this.Properties = new List<Property>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static LuaClosure CreateFunction(Method method) {
            LuaCSharpFuncDelegate funcDelegate = null;
            if (method.MethodAttribute.UseMarshalling) {
                funcDelegate = (state, stack) => LuaMarshal.InvokeAsLua(state, stack, method);
            } else {
                if (method.Info.ReturnType != typeof(int)) {
                    throw new LuaRuntimeError($"Attempt to bind function that does not return {typeof(int).Name}. Set 'UseMarshalling' to true if method should be marshalled.");
                }
                funcDelegate = (state, stack) => (int)method.Info.Invoke(null, new object[] { state, stack });
            }
            if (method.MethodAttribute.CreateMetatable) {
                // pop object from stack and set pointers
            }
            return new LuaClosure(new LuaFunction(funcDelegate));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static LuaTable CreateMetatable(LuaUserObjectType objectType, LuaState state) {
            LuaTable meta = new LuaTable();
            foreach (var property in objectType.Properties) {

            }
            return meta;
        }

    }

}
