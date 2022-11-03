namespace Battlegrounds.Scripting.Lua.Interpreter;

/// <summary>
/// Represents a C (C#) object exposed to Lua. (In this case it's a managed C# object, this also means it'll offer some extra functionality).
/// </summary>
public class LuaUserObject : LuaValue, IMetatableParent {

    private object m_obj;

    /// <summary>
    /// Get the wrapped C# object.
    /// </summary>
    public object Object => this.m_obj;

    /// <summary>
    /// Get or privately set the metatable of the userobject.
    /// </summary>
    public LuaTable MetaTable { get; private set; }

    /// <summary>
    /// Get the type of the encapsulated <see cref="LuaUserObject"/>.
    /// </summary>
    public Type Type => this.m_obj.GetType();

    /// <summary>
    /// Initialize a new <see cref="LuaUserObject"/> class with wrapped C# object.
    /// </summary>
    /// <param name="o">The C# object to represent.</param>
    public LuaUserObject(object o) => this.m_obj = o;

    /// <summary>
    /// Get the wrapped object represented by the User Object.
    /// </summary>
    /// <typeparam name="T">The type to cast the wrapped object into.</typeparam>
    /// <returns>Either the wrapped object or the default value if type parameter is invalid.</returns>
    public T? GetObject<T>() => this.m_obj is T t ? t : default;

    /// <summary>
    /// Set the metatable of the userobject.
    /// </summary>
    /// <param name="meta">The metatable. If not a <see cref="LuaTable"/>, the metatable is set to <see langword="null"/>.</param>
    public void SetMetatable(LuaValue meta) {
        if (meta is LuaTable t) {
            this.MetaTable = t;
        } else {
            this.MetaTable = null;
        }
    }

    public override bool Equals(LuaValue value) => value is LuaUserObject o && o.Type == this.Type && o.m_obj.Equals(this.m_obj);

    public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

    public override int GetHashCode() => this.m_obj.GetHashCode();

    public override string Str() => $"0x{this.m_obj.GetHashCode():X8}";

    public override LuaType GetLuaType() => LuaType.LUA_USEROBJECT;

    public override string ToString() => this.m_obj.ToString();

}
