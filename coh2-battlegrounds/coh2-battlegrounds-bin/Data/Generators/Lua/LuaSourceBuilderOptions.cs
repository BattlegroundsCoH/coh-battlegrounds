using System;
using System.Globalization;

namespace Battlegrounds.Data.Generators.Lua;

/// <summary>
/// Class representing options data for a <see cref="LuaSourceBuilder"/>. This class cannot be inherited.
/// </summary>
public sealed class LuaSourceBuilderOptions {

    /// <summary>
    /// Get the default lua format provider
    /// </summary>
    public IFormatProvider FormatProvider { get; }

    /// <summary>
    /// Get or set if the source builder should write a semicolon when syntax allows. (Defualt: <see langword="true"/>).
    /// </summary>
    public bool WriteSemicolon { get; set; }

    /// <summary>
    /// Get or set if the source builder should write a trailining comma when building tables. (Defualt: <see langword="false"/>).
    /// </summary>
    public bool WriteTrailingComma { get; set; }

    /// <summary>
    /// Get or set the max amount of characters allowed in a table for it to be written in a single line. (Defualt: 64).
    /// </summary>
    public int SingleLineTableLength { get; set; }

    /// <summary>
    /// Get or set the string symbolising a line break. (Default: <see cref="Environment.NewLine"/>).
    /// </summary>
    public string NewLine { get; set; }

    /// <summary>
    /// Get or set if null values should be explicitly saved as nil values in tables. (Default: <see langword="false"/>).
    /// </summary>
    public bool ExplicitNullAsNilValues { get; set; }

    /// <summary>
    /// Get or set if the <see cref="LuaSourceBuilder"/> should apply code verification. (Default: <see langword="true"/>).
    /// </summary>
    public bool CodeVerification { get; set; }

    /// <summary>
    /// Get or set if functions built by the source builder should state it is auto-generated. (Default: <see langword="true"/>).
    /// </summary>
    public bool DenoteGeneratedFunctions { get; set; }

    /// <summary>
    /// Get or set if enums should be trated as numerics if no other behaviour is specified. (Default: <see langword="true"/>).
    /// </summary>
    public bool ByDefaultTreatEnumsAsNumerics { get; set; }

    /// <summary>
    /// Initialsie a new default <see cref="LuaSourceBuilderOptions"/> instance.
    /// </summary>
    public LuaSourceBuilderOptions() {
        WriteSemicolon = true;
        WriteTrailingComma = false;
        SingleLineTableLength = 64;
        NewLine = Environment.NewLine;
        ExplicitNullAsNilValues = false;
        CodeVerification = true;
        DenoteGeneratedFunctions = true;
        ByDefaultTreatEnumsAsNumerics = true;
        FormatProvider = CultureInfo.GetCultureInfo("en-US");
    }

}
