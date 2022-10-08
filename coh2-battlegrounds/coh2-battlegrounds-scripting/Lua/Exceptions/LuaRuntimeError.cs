using Battlegrounds.Scripting.Lua.Interpreter;

namespace Battlegrounds.Scripting.Lua.Exceptions;

/// <summary>
/// Represents errors that occur while running Lua-related code.
/// </summary>
public class LuaRuntimeError : LuaException {

    /// <summary>
    /// Get the stack when exception was thrown.
    /// </summary>
    public LuaStack Stack { get; }

    /// <summary>
    /// Get the state that caused the exception.
    /// </summary>
    public LuaState? State { get; }

    /// <summary>
    /// Initialize a new <see cref="LuaRuntimeError"/> class with a default error message.
    /// </summary>
    public LuaRuntimeError() : base("Fatal and unexpected error occured while executing Lua code.") {
        this.State = null;
        this.Stack = new();
    }

    /// <summary>
    /// Initialize a new <see cref="LuaRuntimeError"/> class with a specialized error message.
    /// </summary>
    /// <param name="luaRuntimeErrMessage">The specialized error message to display when error is thrown.</param>
    public LuaRuntimeError(string luaRuntimeErrMessage) : base(luaRuntimeErrMessage) {
        this.State = null;
        this.Stack = new();
    }

    /// <summary>
    /// Initialize a new <see cref="LuaRuntimeError"/> class with a specialized error message and additional error-state data.
    /// </summary>
    /// <param name="luaRuntimeErrMessage">The specialized error message to display when error is thrown.</param>
    /// <param name="stack">The Lua stack at the time when exception was thrown.</param>
    /// <param name="state">The <see cref="LuaState"/> object that triggered the <see cref="LuaRuntimeError"/>.</param>
    public LuaRuntimeError(string luaRuntimeErrMessage, LuaStack stack, LuaState state) : base(luaRuntimeErrMessage) {
        this.Stack = stack;
        this.State = state;
    }

}

