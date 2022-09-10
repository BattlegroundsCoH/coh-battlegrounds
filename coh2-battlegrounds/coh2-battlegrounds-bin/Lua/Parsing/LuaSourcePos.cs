namespace Battlegrounds.Lua.Parsing;

/// <summary>
/// Represents a position in a Lua source file.
/// </summary>
public readonly struct LuaSourcePos {

    /// <summary>
    /// Undefined source position.
    /// </summary>
    public static readonly LuaSourcePos Undefined = new LuaSourcePos("?.lua", int.MaxValue);

    public readonly string File;
    public readonly int Line;

    /// <summary>
    /// Initialize a new <see cref="LuaSourcePos"/> object with a defined source and line.
    /// </summary>
    /// <param name="file">The filename of the source.</param>
    /// <param name="ln">The line in the source to represent.</param>
    public LuaSourcePos(string file, int ln) {
        this.File = file;
        this.Line = ln;
    }

    public override string ToString() => $"{this.File}:{this.Line}";

}

