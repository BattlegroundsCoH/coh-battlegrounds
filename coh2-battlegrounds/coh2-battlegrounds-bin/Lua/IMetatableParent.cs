namespace Battlegrounds.Lua;

/// <summary>
/// Interface for values that contains a metatable.
/// </summary>
public interface IMetatableParent {

    /// <summary>
    /// Get the metatable of the value.
    /// </summary>
    public LuaTable MetaTable { get; }

}

