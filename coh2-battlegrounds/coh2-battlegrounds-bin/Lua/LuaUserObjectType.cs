using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Runtime;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Type data management class for <see cref="LuaUserObject"/> instances.
    /// </summary>
    public class LuaUserObjectType {

        /// <summary>
        /// Invokable method.
        /// </summary>
        public record Method(string Name, LuaUserobjectMethodAttribute MethodAttribute, MethodInfo Info);

        /// <summary>
        /// Invokable or indexable property.
        /// </summary>
        public record Property(string Name, LuaUserobjectPropertyAttribute PropertyAttribute, PropertyInfo Info);

        /// <summary>
        /// Get the native type that is wrapped.
        /// </summary>
        public Type ObjectType { get; }

        /// <summary>
        /// Get the methods invokable by the type.
        /// </summary>
        public List<Method> Methods { get; }

        /// <summary>
        /// Get the properties invokable or indexable by the type.
        /// </summary>
        public List<Property> Properties { get; }

        /// <summary>
        /// Initialize a new <see cref="LuaUserObjectType"/> bound to specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> the <see cref="LuaUserObjectType"/> is representing.</param>
        public LuaUserObjectType(Type type) {
            this.ObjectType = type;
            this.Methods = new List<Method>();
            this.Properties = new List<Property>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="typeTable"></param>
        /// <exception cref="LuaRuntimeError"/>
        /// <returns></returns>
        public static LuaClosure CreateFunction(LuaUserObjectType objectType, Method method, LuaTable typeTable) {
            
            // The delegate to invoke
            LuaCSharpFuncDelegate funcDelegate = null;

            // Determine if call should be marshalled.
            if (method.MethodAttribute.UseMarshalling) {
                funcDelegate = (state, stack) => LuaMarshal.InvokeAsLua(state, stack, method);
            } else {
                if (method.Info.ReturnType != typeof(int)) {
                    throw new LuaRuntimeError($"Attempt to bind function that does not return {typeof(int).Name}. Set 'UseMarshalling' to true if method should be marshalled.");
                }
                if (!method.Info.IsStatic) {
                    throw new LuaRuntimeError("Attempt to bind a non-marshalled method. Non-marshalled methods must be static.");
                }
                var parms = method.Info.GetParameters();
                if (parms[0].ParameterType != typeof(LuaState)) {
                    throw new LuaRuntimeError($"Attempt to bind a non-marshalled method with incorrect first parameter, found {parms[0].ParameterType.Name}; but expected {nameof(LuaState)}.");
                }
                if (parms[1].ParameterType != typeof(LuaStack)) {
                    throw new LuaRuntimeError($"Attempt to bind a non-marshalled method with incorrect second parameter, found {parms[1].ParameterType.Name}; but expected {nameof(LuaStack)}.");
                }
                funcDelegate = (state, stack) => (int)method.Info.Invoke(null, new object[] { state, stack });
            }

            // If attribute says create metatable, do so
            if (method.MethodAttribute.CreateMetatable) {
                var oldDelegate = funcDelegate;
                funcDelegate = (state, stack) => {
                    if (oldDelegate(state, stack) != 1) {
                        throw new LuaRuntimeError($"Attempt to set metatable of userobject constructor '{method.Name}' that returned multiple values. This is not allowed.");
                    }
                    if (stack.PopOrNil() is LuaUserObject obj) {
                        obj.SetMetatable(CreateMetatable(objectType, state, true));
                        stack.Push(obj); // Push back on stack
                    } else {
                        throw new LuaRuntimeError("Attempt to set metatable of non-metatable valid value.");
                    }
                    return 1;
                };
            }

            // Return closure with resulting delegate
            return new LuaClosure(new LuaFunction(funcDelegate));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static LuaTable CreateMetatable(LuaUserObjectType objectType, LuaState state, bool instanceTable) {
            LuaTable meta = new LuaTable();
            // Define index
            if (instanceTable) {
                meta["__index"] = LuaClosure.Anonymous((state, stack) => {
                    var key = stack.Pop();
                    if (stack.PopOrNil() is LuaUserObject obj && obj.Object.GetType() == objectType.ObjectType) {
                        if (objectType.Properties.FirstOrDefault(x => x.Name == key.Str() && x.Info.GetMethod.IsStatic is false) is Property prop) {
                            stack.Push(LuaMarshal.ToLuaValue(prop.Info.GetMethod.Invoke(obj.Object, Array.Empty<object>())));
                        } else {
                            stack.Push(state._G.ByKey<LuaTable>(objectType.ObjectType.Name)[key]);
                        }
                    } else {
                        stack.Push(new LuaNil()); // Might have to throw error here
                    }
                    return 1;
                });
            } else {
                meta["__index"] = LuaClosure.Anonymous((state, stack) => {
                    var key = stack.Pop();
                    if (stack.PopOrNil() is LuaTable) {
                        if (objectType.Properties.FirstOrDefault(x => x.Name == key.Str() && x.Info.GetMethod.IsStatic is true) is Property prop) {
                            stack.Push(LuaMarshal.ToLuaValue(prop.Info.GetMethod.Invoke(null, Array.Empty<object>())));
                        } else {
                            stack.Push(state._G.ByKey<LuaTable>(objectType.ObjectType.Name)[key]);
                        }
                    } else {
                        stack.Push(new LuaNil()); // Might have to throw error here
                    }
                    return 1;
                });
            }
            return meta;
        }

    }

}
