using System;

namespace Battlegrounds.Lua.Generator.RuntimeServices;

/// <summary>
/// Attribute for marking a specific Lua converter for a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class LuaConverterAttribute : Attribute {

    /// <summary>
    /// Get the converter to use.
    /// </summary>
    public Type ConverterType { get; }

    /// <summary>
    /// Initialise a new <see cref="LuaConverterAttribute"/> instance with the specified converter type.
    /// </summary>
    /// <param name="type">The type of the converter to use.</param>
    public LuaConverterAttribute(Type type) {
        if (!type.IsSubclassOf(typeof(LuaConverter))) {
            throw new ArgumentException($"Invalid type '{type.FullName}'. Must inherit from abstract class '{typeof(LuaConverter)}'.", nameof(type));
        }
        this.ConverterType = type;
    }

    /// <summary>
    /// Create an instance of the specified converter type.
    /// </summary>
    /// <returns>A <see cref="LuaConverter"/> instance.</returns>
    public LuaConverter? CreateConverter() => Activator.CreateInstance(this.ConverterType) as LuaConverter;

}

