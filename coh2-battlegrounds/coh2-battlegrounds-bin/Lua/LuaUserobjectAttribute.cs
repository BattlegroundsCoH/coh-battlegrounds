using System;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Userobject attribute for methods to be directly invoked by the Lua runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaUserobjectMethodAttribute : Attribute {

        /// <summary>
        /// Allow the runtime to invoke the method as is without interacting with Lua values.
        /// </summary>
        public bool UseMarshalling { get; init; } = false;
        
        /// <summary>
        /// Create per instance data after the contained type is returned.
        /// </summary>
        public bool CreateMetatable { get; init; } = false;

    }

    /// <summary>
    /// Userobject attribute for properties to be exposed to the Lua runtime
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LuaUserobjectPropertyAttribute : Attribute {
        public bool IsGetOnly { get; init; } = true;
    }

}
