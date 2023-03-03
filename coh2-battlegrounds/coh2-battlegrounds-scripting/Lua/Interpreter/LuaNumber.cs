using System;
using System.Globalization;

namespace Battlegrounds.Scripting.Lua.Interpreter;

/// <summary>
/// Represents a numeric lua value. Assumes <see cref="double"/> represention. Can be converted to an integer.
/// </summary>
public class LuaNumber : LuaValue {

    /// <summary>
    /// Culture encoding value to use when printing and parsing <see cref="LuaNumber"/> values. Read-only field.
    /// </summary>
    public static readonly CultureInfo NumberCulture = CultureInfo.GetCultureInfo("en-US");

    private readonly double m_number;
    private readonly bool m_treatAsInteger;

    /// <summary>
    /// Initialise a new <see cref="LuaNumber"/> class with <see cref="double"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The floating-point value of the number.</param>
    public LuaNumber(float value) {
        this.m_number = Convert.ToDouble(value.ToString());
        this.m_treatAsInteger = this.IsInteger();
    }

    /// <summary>
    /// Initialise a new <see cref="LuaNumber"/> class with <see cref="double"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The floating-point value of the number.</param>
    public LuaNumber(double value) {
        this.m_number = value;
        this.m_treatAsInteger = this.IsInteger();
    }

    /// <summary>
    /// Initialise a new <see cref="LuaNumber"/> class with an <see cref="int"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The integer value of the number.</param>
    public LuaNumber(int value) {
        this.m_number = value;
        this.m_treatAsInteger = true;
    }

    /// <summary>
    /// Get a new <see cref="LuaNumber"/> where value is an integer representation.
    /// </summary>
    /// <returns>New <see cref="LuaNumber"/>.</returns>
    public LuaNumber AsInteger() => new((int)this.m_number);

    /// <summary>
    /// Get if the numeric represented is an integer.
    /// </summary>
    /// <returns>If number is within epsilon range, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool IsInteger() => (this.m_number % 1) <= double.Epsilon * 100;

    public override bool Equals(LuaValue value) => value is LuaNumber n && n.m_number == this.m_number;

    public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

    public override string Str() => this.ToString();

    public override int GetHashCode() => this.m_number.GetHashCode() ^ this.m_treatAsInteger.GetHashCode();

    public override string ToString() => this.m_treatAsInteger ? ((int)this.m_number).ToString() : this.m_number.ToString(NumberCulture);

    /// <summary>
    /// Returns the integer that best represents the numeric value.
    /// </summary>
    /// <returns>An integer that best represents the numeric value.</returns>
    public int ToInt() => (int)this.m_number;

    public override LuaType GetLuaType() => LuaType.LUA_NUMBER;

    public static explicit operator int(LuaNumber n) => n.IsInteger() ? (int)n.m_number : throw new InvalidCastException("Cannot cast non-integer LuaNumber to an integer");

    public static implicit operator double(LuaNumber n) => n.m_number;

}

