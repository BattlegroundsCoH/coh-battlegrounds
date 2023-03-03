using System;

namespace Battlegrounds.Data.Generators.Lua.RuntimeServices;

/// <summary>
/// Attribute defining the code-generation behaviour of an enum type.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class LuaEnumBehaviourAttribute : Attribute {

    /// <summary>
    /// Get if the enum should be serialised with its string value.
    /// </summary>
    public bool SerialiseAsString { get; }

    /// <summary>
    /// Initialise a new <see cref="LuaEnumBehaviourAttribute"/> instance.
    /// </summary>
    /// <param name="stringify">Should stringify.</param>
    public LuaEnumBehaviourAttribute(bool stringify)
        => this.SerialiseAsString = stringify;

}
