using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Battlegrounds.Data.Generators.Lua.RuntimeServices;

namespace Battlegrounds.Data.Generators.Lua;

/// <summary>
/// Class representing a complex lua source code builder. This class cannot be inherited.
/// </summary>
public sealed class LuaSourceBuilder {

    private readonly Stack<object?> m_contextObjects;

    /// <summary>
    /// Get the current context object
    /// </summary>
    public object? Context => m_contextObjects.Count > 0 ? m_contextObjects.Peek() : null;

    /// <summary>
    /// Get the underlying writer instance that handles the concrete code generation.
    /// </summary>
    public LuaSourceWriter Writer { get; }

    /// <summary>
    /// Get or set the options of the underlying <see cref="LuaSourceWriter"/>.
    /// </summary>
    public LuaSourceBuilderOptions Options {
        get => Writer.Options;
        set => Writer.Options = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public LuaSourceBuilder() {
        Writer = new();
        m_contextObjects = new();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public LuaSourceBuilder Assignment(string variable, object value) {
        Writer.WriteVariableAssignment(variable);
        switch (value) {
            case IDictionary dic:
                Writer.WriteTableValue(BuildTable(dic));
                break;
            case ICollection col:
                Writer.WriteTableValue(BuildTable(col));
                break;
            case string str:
                Writer.WriteStringValue(str);
                break;
            case ValueType vt:
                WriteValue(vt);
                break;
            case object o:
                WriteObject(o);
                break;
            default:
                Writer.WriteNilValue();
                break;
        }
        Writer.EndLine(false);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public LuaSourceBuilder Function(string name, params string[] args) {
        if (Writer.Line > 0) {
            Writer.NewLine();
        }
        if (Options.DenoteGeneratedFunctions) {
            Writer.WriteSingleLineComment("@Auto-Generated");
        }
        Writer.WriteKeyword(LuaKw.Function);
        Writer.WriteVerbatim(name);
        Writer.WriteOperator('(');
        for (int i = 0; i < args.Length; i++) {
            if (!LuaSourceWriter.IsLegalVariableName(args[i])) {
                throw new InvalidOperationException($"Cannot have '{args[i]}' as argument name.");
            }
            Writer.WriteVerbatim(args[i]);
            if (i < args.Length - 1) {
                Writer.WriteOperator(',');
            }
        }
        Writer.WriteOperator(')');
        Writer.EndLine(true);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public LuaSourceBuilder Return() {
        Writer.WriteKeyword(LuaKw.Return);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public LuaSourceBuilder End() {
        Writer.DecreaseIndent();
        if (!Writer.IsEmptyLine) {
            Writer.EndLine(false);
        }
        Writer.WriteKeyword(LuaKw.End);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public LuaSourceBuilder Variables(params string[] vars) {
        for (int i = 0; i < vars.Length; i++) {
            Writer.WriteVerbatim(vars[i]);
            if (i < vars.Length - 1) {
                Writer.WriteOperator(',');
            }
        }
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(string left, char arithmeticOperation, string right) {
        Writer.WriteVerbatim(left);
        Writer.WriteOperator(arithmeticOperation);
        Writer.WriteVerbatim(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(double left, char arithmeticOperation, string right) {
        Writer.WriteNumberValue(left);
        Writer.WriteOperator(arithmeticOperation);
        Writer.WriteVerbatim(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(string left, char arithmeticOperation, double right) {
        Writer.WriteVerbatim(left);
        Writer.WriteOperator(arithmeticOperation);
        Writer.WriteNumberValue(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(double left, char arithmeticOperation, double right) {
        Writer.WriteNumberValue(left);
        Writer.WriteOperator(arithmeticOperation);
        Writer.WriteNumberValue(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(char arithmeticOperation, string right) {
        Writer.WriteOperator(arithmeticOperation);
        Writer.WriteVerbatim(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(char arithmeticOperation, double right) {
        Writer.WriteOperator(arithmeticOperation);
        Writer.WriteNumberValue(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public LuaSourceBuilder Call(string variable, params string[] args) {
        Writer.WriteVerbatim(variable);
        Writer.WriteOperator('(');
        for (int i = 0; i < args.Length; i++) {
            Writer.WriteVerbatim(args[i]);
            if (i < args.Length - 1) {
                Writer.WriteOperator(',');
            }
        }
        Writer.WriteOperator(')');
        return this;
    }

    private string GetKeyName(object obj) => obj switch {
        double d => d.ToString(Options.FormatProvider),
        float f => f.ToString(Options.FormatProvider),
        int i32 => i32.ToString(Options.FormatProvider),
        short i16 => i16.ToString(Options.FormatProvider),
        long i64 => i64.ToString(Options.FormatProvider),
        sbyte i8 => i8.ToString(Options.FormatProvider),
        byte ui8 => ui8.ToString(Options.FormatProvider),
        ushort ui16 => ui16.ToString(Options.FormatProvider),
        uint ui32 => ui32.ToString(Options.FormatProvider),
        ulong ui64 => ui64.ToString(Options.FormatProvider),
        string s => s,
        char c => c.ToString(),
        bool b => b ? "true" : "false",
        _ => throw new KeyNotFoundException(),
    };

    private Table BuildTable(IEnumerable enumerable) {

        // Create table
        Table table = new();
        int i = 1;
        foreach (var item in enumerable) {
            if (item is null && Options.ExplicitNullAsNilValues) {
                table[i] = null;
            } else if (item is not null && GetTableValue(item) is object v) {
                table[i] = v;
            }
            i++;
        }

        // Return result
        return table;

    }

    private Table BuildTable(IDictionary dictionary) {

        // Create table
        Table table = new();
        bool pop = false;

        // Loop over entries
        foreach (DictionaryEntry entry in dictionary) {
            if (entry.Key is "__@luacontext") {
                m_contextObjects.Push(entry.Value);
                pop = true;
            } else {
                if (entry.Value is null && Options.ExplicitNullAsNilValues) {
                    table[GetKeyName(entry.Key)] = null;
                } else if (entry.Value is not null && GetTableValue(entry.Value) is object lv) {
                    table[GetKeyName(entry.Key)] = lv;
                }
            }
        }

        // Reset context object if any was pushed
        if (pop)
            m_contextObjects.Pop();

        // Return result
        return table;

    }

    private object GetEnumLuaValue(Enum e) {
        bool asString = e.GetType().GetCustomAttribute<LuaEnumBehaviourAttribute>() is LuaEnumBehaviourAttribute eb && eb.SerialiseAsString;
        if (asString || !Options.ByDefaultTreatEnumsAsNumerics) {
            return e.ToString();
        } else if (e.GetType()?.BaseType is Type btype) {
            return Convert.ChangeType(e, btype);
        }
        throw new Exception($"Failed to get enum value {e} as lua value.");
    }

    private object GetObjectAsTable(object obj) {
        if (HasConverter(obj, out LuaConverter? converter)) {
            return new LuaLazyConverter(this, converter, obj);
        }
        return BuildTable(obj);
    }

    private object GetValue(ValueType vt) {
        if (HasConverter(vt, out LuaConverter? converter)) {
            return new LuaLazyConverter(this, converter, vt);
        }
        return vt;
    }

    private object? GetTableValue(object? value) => value switch {
        Enum e => GetEnumLuaValue(e),
        string s => s,
        IDictionary dictionary => BuildTable(dictionary),
        IEnumerable enumerable => BuildTable(enumerable),
        ValueType vt => GetValue(vt),
        null when this.Options.ExplicitNullAsNilValues => null,
        not null => GetObjectAsTable(value),
        _ => null
    };

    private Table BuildTable(object obj) {

        // Create table
        Table table = new();

        // Get type
        var type = obj.GetType();

        // Yay reflection...
        var publicProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        // Add Properties
        foreach (var property in publicProperties) {

            // Make sure we're not supposed to ignore this
            if (property.GetCustomAttribute<LuaIgnoreAttribute>() is null) {

                // Get table value and key
                object? tableValue = property.GetValue(obj);
                string tableKey = property.GetCustomAttribute<LuaNameAttribute>() is LuaNameAttribute name ? name.Name : property.Name;

                // If converted, save
                if (this.GetTableValue(tableValue) is object val) {
                    table[tableKey] = val;
                }

            }

        }

        // Return result
        return table;

    }

    private static bool HasConverter(object obj, [NotNullWhen(true)] out LuaConverter? luaConverter) {
        if (obj.GetType().GetCustomAttribute<LuaConverterAttribute>() is LuaConverterAttribute luaConverterAttribute) {
            luaConverter = luaConverterAttribute.CreateConverter();
            return luaConverter is not null;
        }
        luaConverter = null;
        return false;
    }

    private void WriteObject(object obj) {

        // Check if there's a converter in place
        if (HasConverter(obj, out LuaConverter? converter)) {

            // Get type to convert
            var objType = obj.GetType();

            // Make sure we can write to the type.
            if (!converter.CanWrite(objType)) {
                throw new InvalidOperationException($"Cannot use converter '{converter.GetType().FullName}' on type '{objType.FullName}'.");
            }

            // Write
            converter.Write(this, obj);

        } else {

            // Write object as raw table.
            Writer.WriteTableValue(BuildTable(obj));

        }

    }

    private void WriteValue(ValueType vt) {

        // Check if there's a converter in place
        if (HasConverter(vt, out LuaConverter? converter)) {

            // Get type to convert
            var objType = vt.GetType();

            // Make sure we can write to the type.
            if (!converter.CanWrite(objType)) {
                throw new InvalidOperationException($"Cannot use converter '{converter.GetType().FullName}' on type '{objType.FullName}'.");
            }

            // Write
            converter.Write(this, vt);


        } else {

            // Just write the value
            Writer.WriteValue(vt);

        }

    }

    /// <summary>
    /// Build the raw Lua table version of an object without converter-specified procedures.
    /// </summary>
    /// <param name="obj">The object to build table representation of.</param>
    /// <returns>The <see cref="LuaTable"/> representation of <paramref name="obj"/>.</returns>
    public Table BuildTableRaw(object obj)
        => BuildTable(obj);

    /// <summary>
    /// Build the raw Lua table version of an object without converter-specified procedures.
    /// </summary>
    /// <param name="dict">The object to build table representation of.</param>
    /// <returns>The <see cref="LuaTable"/> representation of <paramref name="dict"/>.</returns>
    public Table BuildTableRaw(IDictionary<string, object> dict)
        => BuildTable((IDictionary)dict);

    /// <summary>
    /// Build the raw Lua table version of an object without converter-specified procedures.
    /// </summary>
    /// <typeparam name="TValue">The type of the dictionary value</typeparam>
    /// <param name="dict">The object to build table representation of.</param>
    /// <returns>The <see cref="LuaTable"/> representation of <paramref name="dict"/>.</returns>
    public Table BuildTableRaw<TValue>(IDictionary<string, TValue> dict)
        => BuildTable((IDictionary)dict);

    /// <summary>
    /// Get the source text described by the <see cref="Writer"/>.
    /// </summary>
    /// <returns>Complete Lue source code text.</returns>
    public string GetSourceText() => Writer.GetContent();

}
