using System;

namespace Battlegrounds.Lua.Generator.RuntimeServices;

/// <summary>
/// Attribute class telling the automated <see cref="LuaSourceBuilder"/> to ignore the property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LuaIgnoreAttribute : Attribute {

    /// <summary>
    /// Initialise a new <see cref="LuaIgnoreAttribute"/> instance.
    /// </summary>
    public LuaIgnoreAttribute() { }

}

