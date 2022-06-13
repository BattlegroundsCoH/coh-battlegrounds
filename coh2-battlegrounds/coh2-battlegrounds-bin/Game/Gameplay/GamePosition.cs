using System.Text.Json.Serialization;

using Battlegrounds.Lua.Generator.RuntimeServices;

namespace Battlegrounds.Game; 

/// <summary>
/// Represents a world position in the game using X,Y,Z variables
/// </summary>
public struct GamePosition {

    private double m_x;
    private double m_y;
    private double m_z;

    /// <summary>
    /// The X-coordinate
    /// </summary>
    [LuaName("x")]
    public double X {
        get => this.m_x; 
        set => this.m_x = value; 
    }

    /// <summary>
    /// The Y-coordinate
    /// </summary>
    [LuaName("y")]
    public double Y {
        get => this.m_y; 
        set => this.m_y = value;
    }

    /// <summary>
    /// The Z-coordinate
    /// </summary>
    [LuaName("z")]
    public double Z {
        get => this.m_z; 
        set => this.m_z = value;
    }

    /// <summary>
    /// New instance of a <see cref="GamePosition"/> using only the two first coordinates (XY) - a 2D position
    /// </summary>
    /// <param name="x">The X-coordinate</param>
    /// <param name="y">The Y-coordinate</param>
    public GamePosition(double x, double y) : this() {
        this.m_x = x;
        this.m_y = y;
    }

    /// <summary>
    /// New instance of a <see cref="GamePosition"/> using all three coordinates (XYZ) - a 3D position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    [JsonConstructor]
    public GamePosition(double X, double Y, double Z) : this(X, Y) {
        this.m_z = Z;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"({this.X}, {this.Y}, {this.Z})";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public (double w, double h) ToTuple2() => (this.X, this.Y);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Deconstruct(out double x, out double y) {
        x = this.m_x;
        y = this.m_y;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Deconstruct(out double x, out double y, out double z) {
        x = this.m_x;
        y = this.m_y;
        z = this.m_z;
    }

    /// <summary>
    /// The game position at (0, 0, 0)
    /// </summary>
    public static readonly GamePosition Naught = new GamePosition(0.0f, 0.0f, 0.0f);

}
