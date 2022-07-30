using System;
using System.Globalization;
using System.Text.Json.Serialization;

using Battlegrounds.Game.Gameplay.DataConverters;
using Battlegrounds.Lua.Generator.RuntimeServices;
using Battlegrounds.Util;

namespace Battlegrounds.Game;

/// <summary>
/// Represents a world position in the game using X,Y,Z variables
/// </summary>
[LuaConverter(typeof(PositionConverter))]
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
    public override string ToString() 
        => $"({this.X.ToString(CultureInfo.InvariantCulture)}, {this.Y.ToString(CultureInfo.InvariantCulture)}, {this.Z.ToString(CultureInfo.InvariantCulture)})";

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

    /// <summary>
    /// Compute the squared distance between this point and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other point to compute squared distance to.</param>
    /// <returns>The squared distance between the two points.</returns>
    public readonly double SquareDistance(GamePosition other)
        => (this.m_x - other.m_x).Square() + (this.m_y - other.m_y).Square() + (this.m_z - other.m_z).Square();

    /// <summary>
    /// Interpolate between this position and <paramref name="y"/>.
    /// </summary>
    /// <param name="y">The other position to interpolate with.</param>
    /// <param name="t">
    /// The point to interpolate at. For 0 ≤ <paramref name="t"/> ≤ 1 the point will be between this and <paramref name="t"/>.
    /// </param>
    /// <returns>The interpolated position.</returns>
    public GamePosition Interpolate(GamePosition y, double t) => this * t + y * (1 - t);

    /// <summary>
    /// Get a new <see cref="GamePosition"/> with swapped Y and Z coordinates.
    /// </summary>
    /// <returns>A copy with the Y and Z coordinates swap.</returns>
    public GamePosition SwapYZ() => this with { m_y = this.m_z, m_z = this.m_y };

    /// <summary>
    /// Get a random new <see cref="GamePosition"/> within <paramref name="radius"/>.
    /// </summary>
    /// <param name="radius">The radius to pick new point.</param>
    /// <param name="rng">The random number generator. If none is specified, <see cref="BattlegroundsInstance.RNG"/> is used.</param>
    /// <returns>A position randomly offset inside the <paramref name="radius"/> of the position.</returns>
    public GamePosition RandomOffset(double radius, Random? rng = null) {
        if (rng is null) {
            rng = BattlegroundsInstance.RNG;
        }
        return this with {
            m_x = this.m_x + rng.NextDouble() * (2 * radius) - radius,
            m_y = this.m_y + rng.NextDouble() * (2 * radius) - radius,
            m_z = this.m_z + rng.NextDouble() * (2 * radius) - radius
        };
    }

    public static GamePosition operator *(GamePosition a, double b)
        => new(a.m_x * b, a.m_y * b, a.m_z * b);

    public static GamePosition operator +(GamePosition a, GamePosition b)
        => new(a.m_x + b.m_x, a.m_y + b.m_y, a.m_z + b.m_z);

}
