using System;

namespace Battlegrounds.Lua.Generator.RuntimeServices;

/// <summary>
/// Abstract class for converting some managed object into a Lua script representation.
/// </summary>
public abstract class LuaConverter {

    /// <summary>
    /// Get if <paramref name="type"/> can be converted by the concrete <see cref="LuaConverter"/> instance.
    /// </summary>
    /// <param name="type">The type to check if convertible.</param>
    /// <returns>If converter can write the type, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public abstract bool CanWrite(Type type);

    /// <summary>
    /// Write <paramref name="value"/> into its lua representation using <paramref name="luaSourceBuilder"/>.
    /// </summary>
    /// <param name="luaSourceBuilder">The source builder instance to build representation with.</param>
    /// <param name="value">The object to write</param>
    public abstract void Write(LuaSourceBuilder luaSourceBuilder, object value);

}

/// <summary>
/// Abstract class for converting <typeparamref name="T"/> into the specific Lua script represention.
/// </summary>
/// <typeparam name="T">The managed type to convert.</typeparam>
public abstract class LuaConverter<T> : LuaConverter {

    public override bool CanWrite(Type type)
        => typeof(T) == type;

    /// <summary>
    /// Write <paramref name="value"/> into its lua representation using <paramref name="luaSourceBuilder"/>.
    /// </summary>
    /// <param name="luaSourceBuilder">The source builder instance to build representation with.</param>
    /// <param name="value">The specific <typeparamref name="T"/> instance to convert.</param>
    public sealed override void Write(LuaSourceBuilder luaSourceBuilder, object value) => this.Write(luaSourceBuilder, (T)value);

    /// <summary>
    /// Write <paramref name="value"/> into its lua representation using <paramref name="luaSourceBuilder"/>.
    /// </summary>
    /// <param name="luaSourceBuilder">The source builder instance to build representation with.</param>
    /// <param name="value">The specific <typeparamref name="T"/> instance to convert.</param>
    public abstract void Write(LuaSourceBuilder luaSourceBuilder, T value);

}

