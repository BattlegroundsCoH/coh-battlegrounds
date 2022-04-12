using System;

namespace Battlegrounds.Lua.Debugging {

    /// <summary>
    /// Represents errors that occur while handling Lua-related code.
    /// </summary>
    public class LuaException : Exception {

        /// <summary>
        /// Initialize a new <see cref="LuaException"/> class with a default error message.
        /// </summary>
        public LuaException() : base("Fatal Lua error has occured") { }

        /// <summary>
        /// Initialize a new <see cref="LuaException"/> class with a specified error message.
        /// </summary>
        /// <param name="luaErrMessage">The custom (more detailed) error message to show.</param>
        public LuaException(string luaErrMessage) : base(luaErrMessage) { }

    }

}
