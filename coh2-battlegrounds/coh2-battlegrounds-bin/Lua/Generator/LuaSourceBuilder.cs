using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Battlegrounds.Lua.Generator.RuntimeServices;

namespace Battlegrounds.Lua.Generator;

/// <summary>
/// Class representing a complex lua source code builder. This class cannot be inherited.
/// </summary>
public sealed class LuaSourceBuilder {

    private readonly Stack<object?> m_contextObjects;

    /// <summary>
    /// Get the current context object
    /// </summary>
    public object? Context => this.m_contextObjects.Count > 0 ? this.m_contextObjects.Peek() : null;

    /// <summary>
    /// Get the underlying writer instance that handles the concrete code generation.
    /// </summary>
    public LuaSourceWriter Writer { get; }

    /// <summary>
    /// Get or set the options of the underlying <see cref="LuaSourceWriter"/>.
    /// </summary>
    public LuaSourceBuilderOptions Options {
        get => this.Writer.Options;
        set => this.Writer.Options = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public LuaSourceBuilder() {
        this.Writer = new();
        this.m_contextObjects = new();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public LuaSourceBuilder Assignment(string variable, object value) {
        this.Writer.WriteVariableAssignment(variable);
        switch (value) {
            case IDictionary dic:
                this.Writer.WriteTableValue(this.BuildTable(dic));
                break;
            case ICollection col:
                this.Writer.WriteTableValue(this.BuildTable(col));
                break;
            case string str:
                this.Writer.WriteStringValue(str);
                break;
            case ValueType vt:
                this.WriteValue(vt);
                break;
            case object o:
                this.WriteObject(o);
                break;
            default:
                this.Writer.WriteNilValue();
                break;
        }
        this.Writer.EndLine(false);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public LuaSourceBuilder Function(string name, params string[] args) {
        if (this.Writer.Line > 0) {
            this.Writer.NewLine();
        }
        if (this.Options.DenoteGeneratedFunctions) {
            this.Writer.WriteSingleLineComment("@Auto-Generated");
        }
        this.Writer.WriteKeyword(LuaKw.Function);
        this.Writer.WriteVerbatim(name);
        this.Writer.WriteOperator('(');
        for (int i = 0; i < args.Length; i++) {
            if (!LuaSourceWriter.IsLegalVariableName(args[i])) {
                throw new InvalidOperationException($"Cannot have '{args[i]}' as argument name.");
            }
            this.Writer.WriteVerbatim(args[i]);
            if (i < args.Length - 1) {
                this.Writer.WriteOperator(',');
            }
        }
        this.Writer.WriteOperator(')');
        this.Writer.EndLine(true);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public LuaSourceBuilder Return() {
        this.Writer.WriteKeyword(LuaKw.Return);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public LuaSourceBuilder End() {
        this.Writer.DecreaseIndent();
        if (!this.Writer.IsEmptyLine) {
            this.Writer.EndLine(false);
        }
        this.Writer.WriteKeyword(LuaKw.End);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public LuaSourceBuilder Variables(params string[] vars) {
        for (int i = 0; i < vars.Length; i++) {
            this.Writer.WriteVerbatim(vars[i]);
            if (i < vars.Length - 1) {
                this.Writer.WriteOperator(',');
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
        this.Writer.WriteVerbatim(left);
        this.Writer.WriteOperator(arithmeticOperation);
        this.Writer.WriteVerbatim(right);
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
        this.Writer.WriteNumberValue(left);
        this.Writer.WriteOperator(arithmeticOperation);
        this.Writer.WriteVerbatim(right);
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
        this.Writer.WriteVerbatim(left);
        this.Writer.WriteOperator(arithmeticOperation);
        this.Writer.WriteNumberValue(right);
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
        this.Writer.WriteNumberValue(left);
        this.Writer.WriteOperator(arithmeticOperation);
        this.Writer.WriteNumberValue(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(char arithmeticOperation, string right) {
        this.Writer.WriteOperator(arithmeticOperation);
        this.Writer.WriteVerbatim(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arithmeticOperation"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public LuaSourceBuilder Arithmetic(char arithmeticOperation, double right) {
        this.Writer.WriteOperator(arithmeticOperation);
        this.Writer.WriteNumberValue(right);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public LuaSourceBuilder Call(string variable, params string[] args) {
        this.Writer.WriteVerbatim(variable);
        this.Writer.WriteOperator('(');
        for (int i = 0; i < args.Length; i++) {
            this.Writer.WriteVerbatim(args[i]);
            if (i < args.Length - 1) {
                this.Writer.WriteOperator(',');
            }
        }
        this.Writer.WriteOperator(')');
        return this;
    }

    private string GetKeyName(object obj) => obj switch {
        double d => d.ToString(this.Options.FormatProvider),
        float f => f.ToString(this.Options.FormatProvider),
        int i32 => i32.ToString(this.Options.FormatProvider),
        short i16 => i16.ToString(this.Options.FormatProvider),
        long i64 => i64.ToString(this.Options.FormatProvider),
        sbyte i8 => i8.ToString(this.Options.FormatProvider),
        byte ui8 => ui8.ToString(this.Options.FormatProvider),
        ushort ui16 => ui16.ToString(this.Options.FormatProvider),
        uint ui32 => ui32.ToString(this.Options.FormatProvider),
        ulong ui64 => ui64.ToString(this.Options.FormatProvider),
        string s => s,
        char c => c.ToString(),
        bool b => b ? "true" : "false",
        _ => throw new KeyNotFoundException(),
    };

    private LuaTable BuildTable(IEnumerable enumerable) {

        // Create table
        LuaTable table = new();
        int i = 1;
        foreach (var item in enumerable) {
            if (item is null && this.Options.ExplicitNullAsNilValues) {
                table[i] = LuaNil.Nil;
            } else if (item is not null && this.GetTableValue(item) is LuaValue v) {
                table[i] = v;
            }
            i++;
        }

        // Return result
        return table;

    }

    private LuaTable BuildTable(IDictionary dictionary) {

        // Create table
        LuaTable table = new();
        bool pop = false;

        // Loop over entries
        foreach (DictionaryEntry entry in dictionary) {
            if (entry.Key is "__@luacontext") {
                this.m_contextObjects.Push(entry.Value);
                pop = true;
            } else {
                if (entry.Value is null && this.Options.ExplicitNullAsNilValues) {
                    table[this.GetKeyName(entry.Key)] = LuaNil.Nil;
                } else if (entry.Value is not null && this.GetTableValue(entry.Value) is LuaValue lv) {
                    table[this.GetKeyName(entry.Key)] = lv;
                }
            }
        }

        // Reset context object if any was pushed
        if (pop)
            this.m_contextObjects.Pop();

        // Return result
        return table;

    }

    private LuaValue GetEnumLuaValue(Enum e) {
        bool asString = e.GetType().GetCustomAttribute<LuaEnumBehaviourAttribute>() is LuaEnumBehaviourAttribute eb && eb.SerialiseAsString;
        if (asString || !this.Options.ByDefaultTreatEnumsAsNumerics) {
            return new LuaString(e.ToString());
        } else if (e.GetType()?.BaseType is Type btype) {
            return LuaMarshal.ToLuaValue(Convert.ChangeType(e, btype));
        }
        throw new Exception($"Failed to get enum value {e} as lua value.");
    }

    private LuaValue GetObjectAsTable(object obj) {
        if (HasConverter(obj, out LuaConverter? converter)) {
            return new LuaUserObject(new LuaLazyConverter(this, converter, obj));
        }
        return this.BuildTable(obj);
    }

    private LuaValue GetValue(ValueType vt) {
        if (HasConverter(vt, out LuaConverter? converter)) {
            return new LuaUserObject(new LuaLazyConverter(this, converter, vt));
        }
        return LuaMarshal.ToLuaValue(vt);
    }

    private LuaValue? GetTableValue(object? value) => value switch {
        Enum e => this.GetEnumLuaValue(e),
        string => LuaMarshal.ToLuaValue(value),
        IDictionary dictionary => this.BuildTable(dictionary),
        IEnumerable enumerable => this.BuildTable(enumerable),
        ValueType vt => this.GetValue(vt),
        null when this.Options.ExplicitNullAsNilValues => LuaNil.Nil,
        not null => this.GetObjectAsTable(value),
        _ => null
    };

    private LuaTable BuildTable(object obj) {

        // Create table
        LuaTable table = new();

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
                if (this.GetTableValue(tableValue) is LuaValue val) {
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
            this.Writer.WriteTableValue(this.BuildTable(obj));

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
            this.Writer.WriteValue(vt);

        }

    }

    /// <summary>
    /// Build the raw Lua table version of an object without converter-specified procedures.
    /// </summary>
    /// <param name="obj">The object to build table representation of.</param>
    /// <returns>The <see cref="LuaTable"/> representation of <paramref name="obj"/>.</returns>
    public LuaTable BuildTableRaw(object obj)
        => this.BuildTable(obj);

    /// <summary>
    /// Build the raw Lua table version of an object without converter-specified procedures.
    /// </summary>
    /// <param name="dict">The object to build table representation of.</param>
    /// <returns>The <see cref="LuaTable"/> representation of <paramref name="dict"/>.</returns>
    public LuaTable BuildTableRaw(IDictionary<string, object> dict)
        => this.BuildTable((IDictionary)dict);

    /// <summary>
    /// Build the raw Lua table version of an object without converter-specified procedures.
    /// </summary>
    /// <typeparam name="TValue">The type of the dictionary value</typeparam>
    /// <param name="dict">The object to build table representation of.</param>
    /// <returns>The <see cref="LuaTable"/> representation of <paramref name="dict"/>.</returns>
    public LuaTable BuildTableRaw<TValue>(IDictionary<string, TValue> dict)
        => this.BuildTable((IDictionary)dict);

    /// <summary>
    /// Get the source text described by the <see cref="Writer"/>.
    /// </summary>
    /// <returns>Complete Lue source code text.</returns>
    public string GetSourceText() => this.Writer.GetContent();

}
