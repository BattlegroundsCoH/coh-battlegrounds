using System;

namespace Battlegrounds.Lua.Generator.RuntimeServices;

/// <summary>
/// Attribute class defining a different name of a property in Lua code.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LuaNameAttribute : Attribute {

    /// <summary>
    /// Get the Lua name of the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initialsie a new <see cref="LuaNameAttribute"/> with a defined <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The lua name of the property.</param>
    public LuaNameAttribute(string name) => this.Name = name;

}

