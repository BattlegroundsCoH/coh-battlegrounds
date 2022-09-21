using System;
using System.Diagnostics;
using System.Text;

using Battlegrounds.Lua.Debugging;

namespace Battlegrounds.Lua.Runtime;

/// <summary>
/// Base lua library functionality
/// </summary>
public static class LuaBaseLib {

    /// <summary>
    /// Basic print functionality. Prints to <see cref="LuaState.Out"/> and <see cref="Trace"/> if <see cref="LuaState.EnableTrace"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    public static void Print(LuaState state, LuaStack stack) {
        LuaValue top = stack.PopOrNil();
        string message = top.Str(); // TODO: Call __tostring
        if (stack.Any) {
            StringBuilder builder = new StringBuilder(message);
            while (stack.Any) {
                builder.Append(' ');
                builder.Append(stack.Pop().Str()); // TODO: Call __tostring
            }
            message = builder.ToString();
        }
        state.Out?.WriteLine(message);
        if (state.EnableTrace) {
            Trace.WriteLine(message, "Lua");
        }
    }

    /// <summary>
    /// Halts the execution of the <see cref="LuaState"/> with a potential error message.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <exception cref="LuaRuntimeError"/>
    public static void Error(LuaState state, LuaStack stack) {
        var top = stack.PopOrNil();
        if (top is LuaString or LuaNumber) {
            throw new LuaRuntimeError(top.Str());
        } else {
            throw new LuaRuntimeError($"error object is a {top.GetLuaType()} value");
        }
    }

    /// <summary>
    /// Get the next key and value in a table.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int Next(LuaState state, LuaStack stack) {
        LuaValue current = stack.PopOrNil();
        LuaValue top = stack.PopOrNil();
        if (top is LuaTable t) {
            var next = t.Next(current, out LuaValue i);
            stack.Push(next);
            stack.Push(i);
        } else {
            throw new LuaRuntimeError($"Expected table, found {top.GetLuaType()} value.");
        }
        return 2;
    }

    /// <summary>
    /// Get the next index and value based on current index.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int Iter(LuaState state, LuaStack stack) {
        if (stack.Pop() is LuaNumber i && stack.Pop() is LuaTable t) {
            i = new LuaNumber(i + 1);
            var v = t[i];
            if (v is not LuaNil) {
                stack.Push(v);
                stack.Push(i);
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
    /// Pushes basic index-iterator data onto stack for a given table.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
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
    /// Pushes key-value iterator data onto stack for a given table.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int Pairs(LuaState state, LuaStack stack) {
        if (stack.Pop() is LuaTable t) {
            stack.Push(LuaNil.Nil);
            stack.Push(t);
            stack.Push(state._G["next"]);
        } else {
            throw new LuaRuntimeError();
        }
        return 3;
    }

    /// <summary>
    /// Pop first stack element and converts it into a string.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int ToString(LuaState state, LuaStack stack) {
        stack.Push(new LuaString(stack.PopOrNil().Str()));
        return 1;
    }

    /// <summary>
    /// Pop first stack element and attempts to convert it into a number.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int ToNumber(LuaState state, LuaStack stack) {
        var top = stack.PopOrNil();
        if (top.GetLuaType() == LuaType.LUA_STRING && double.TryParse(top.Str(), out double d)) {
            stack.Push(new LuaNumber(d));
        } else if (top is LuaNumber) {
            stack.Push(top);
        } else {
            stack.Push(LuaNil.Nil);
        }
        return 1;
    }

    /// <summary>
    /// Pop first stack element and returns the string name of value type.
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int TypeOf(LuaState state, LuaStack stack) {
        stack.Push(new LuaString(stack.PopOrNil().GetLuaType().LuaTypeName));
        return 1;
    }

    /// <summary>
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int ProtectedCall(LuaState state, LuaStack stack)
        => throw new NotImplementedException(); // Catch, return, and continue

    /// <summary>
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int XProtectedCall(LuaState state, LuaStack stack)
        => throw new NotImplementedException(); // Catch, return, and give error to a handler function - then continue

    /// <summary>
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int Assert(LuaState state, LuaStack stack)
        => throw new NotImplementedException();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int SetMetaTable(LuaState state, LuaStack stack) {
        if (stack.PopOrNil() is LuaValue meta) {
            if (stack.PopOrNil() is LuaTable table) {
                table["__metatable"] = meta;
                stack.Push(table);
                return 1;
            }
        }
        throw new LuaRuntimeError();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state">The currently executing <see cref="LuaState"/>.</param>
    /// <param name="stack">The current stack values</param>
    /// <returns>The amount of new values pushed to the stack.</returns>
    public static int GetMetaTable(LuaState state, LuaStack stack) {
        if (stack.PopOrNil() is LuaTable table) {
            stack.Push(table.MetaTable);
            return 1;
        } else {
            throw new LuaRuntimeError();
        }
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
        lState.RegisterFunction("type", TypeOf);

        // Register metadata functions
        lState.RegisterFunction("setmetatable", SetMetaTable);
        lState.RegisterFunction("getmetatable", GetMetaTable);

        // Register iteration functions
        lState.RegisterFunction("next", Next);
        lState.RegisterFunction("ipairs", IPairs);
        lState.RegisterFunction("pairs", Pairs);

        // Register do/load functions

        // Basic error handling
        lState.RegisterFunction("pcall", ProtectedCall);
        lState.RegisterFunction("xpcall", XProtectedCall);
        lState.RegisterFunction("assert", Assert);

    }

}

