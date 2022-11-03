using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Battlegrounds.Data.Generators.Lua.RuntimeServices;
using Battlegrounds.Functional;

namespace Battlegrounds.Data.Generators.Lua;

public enum LuaKw {
    Return,
    End,
    If,
    ElseIf,
    Else,
    Then,
    For,
    Do,
    While,
    Break,
    Function,
    Repeat,
    Until,
    In
}

public class LuaSourceWriter {

    private static readonly Regex VarRegex = new(@"^[A-z_]+([A-z0-9_])*$");

    private readonly StringBuilder m_builder;
    private int m_indent;
    private int m_line;

    public bool IsEmptyLine => this.m_builder.Length is 0 || this.m_builder[^1] is '\n' or '\t';

    public int Line => this.m_line;

    public LuaSourceBuilderOptions Options { get; set; }

    public LuaSourceWriter() {
        Options = new();
        m_builder = new();
        m_indent = 0;
        m_line = 0;
    }

    public void IncreaseIndent() => this.m_indent++;

    public void DecreaseIndent() => this.m_indent--;

    public void NewLine() {
        this.m_builder.Append(this.Options.NewLine).Append('\t', this.m_indent);
        this.m_line++;
    }

    public void EndLine(bool ignoreSemicolonAnyways) {
        if (!ignoreSemicolonAnyways && this.Options.WriteSemicolon && !IsEmptyLine) {
            this.m_builder.Append(';');
        }
        this.NewLine();
    }

    public void WriteSingleLineComment(string text) {
        _ = this.m_builder.Append("-- ").Append(text);
        this.EndLine(true);
    }

    public void WriteKeyword(LuaKw keyword) {
        _ = this.m_builder.Append(keyword.ToString().ToLowerInvariant());
        if (keyword is not LuaKw.End) {
            _ = this.m_builder.Append(' ');
        }
        bool isEnd = keyword is LuaKw.End;
        if (isEnd || keyword is LuaKw.Then) {
            NewLine(); // Will always end line                
        }
        if (keyword is LuaKw.For or LuaKw.Function or LuaKw.If or LuaKw.Else or LuaKw.ElseIf) {
            this.m_indent++; // Will always increase indent
        }
    }

    public void WriteOperator(char op) {
        _ = op switch {
            '=' or '+' or '-' or '*' or '/' or '^' => this.m_builder.Append(' ').Append(op).Append(' '),
            ',' => this.m_builder.Append(", "),
            '(' or ')' => this.m_builder.Append(op),
            _ => throw new InvalidOperationException($"'{op}' is not a valid operator."),
        };
    }

    public void WriteVerbatim(string verbatim) => this.m_builder.Append(verbatim);

    public void WriteVariable(string variable)
        => this.m_builder.Append(variable);

    public void WriteVariableAssignment(string variable) {
        _ = this.m_builder.Append(variable);
        WriteOperator('=');
    }

    public void WriteStringValue(string value)
        => this.m_builder.Append('"').Append(value).Append('"');

    public void WriteNumberValue(byte value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(sbyte value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(ushort value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(short value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(long value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(ulong value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(uint value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(int value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(float value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteNumberValue(double value)
        => this.m_builder.Append(value.ToString(this.Options.FormatProvider));

    public void WriteBooleanValue(bool value)
        => this.m_builder.Append(value is true ? "true" : "false");

    public void WriteNilValue()
        => this.m_builder.Append("nil");

    public void WriteValue(object? value) {
        switch (value) {
            case string s:
                WriteStringValue(s);
                break;
            case byte ui8:
                WriteNumberValue(ui8);
                break;
            case sbyte i8:
                WriteNumberValue(i8);
                break;
            case short i16:
                WriteNumberValue(i16);
                break;
            case int i32:
                WriteNumberValue(i32);
                break;
            case long i64:
                WriteNumberValue(i64);
                break;
            case ushort ui16:
                WriteNumberValue(ui16);
                break;
            case uint ui32:
                WriteNumberValue(ui32);
                break;
            case ulong ui64:
                WriteNumberValue(ui64);
                break;
            case float s32:
                WriteNumberValue(s32);
                break;
            case double s64:
                WriteNumberValue(s64);
                break;
            case bool b:
                WriteBooleanValue(b);
                break;
            case char c:
                WriteStringValue(c.ToString());
                break;
            default:
                WriteNilValue();
                break;
        }
    }

    private int CalculateLengthOfTableEntries(Table table) {
        int sum = 0;
        foreach (var tableEntry in table) {
            sum += tableEntry.Key.ToString()!.Length + 7;
            if (tableEntry.Value is Table subTable) {
                sum += CalculateLengthOfTableEntries(subTable);
            } else {
                sum += tableEntry.Value?.ToString()!.Length ?? 3;
            }
            sum++; // for the potential comma
            if (sum > Options.SingleLineTableLength) {
                return sum + Options.SingleLineTableLength;
            }
        }
        return sum;
    }

    public void WriteTableValue(Table table) {
        if (table.Size is 0) {
            _ = this.m_builder.Append("{}");
            return;
        }
        bool isArray = table.IsArray();
        int pseudoSize = this.m_indent * 4 + CalculateLengthOfTableEntries(table);
        bool singleLine = pseudoSize <= this.Options.SingleLineTableLength;
        bool isFields = !isArray && table.StringKeys.All(IsLegalVariableName);
        _ = this.m_builder.Append('{').IfFalse(_ => singleLine)
            .Then(q => { m_indent++; NewLine(); })
            .Else(q => q.Append(' '));
        var entries = table.ToArray();
        for (int i = 0; i < entries.Length; i++) {
            var tableEntry = entries[i];
            if (!isArray) {
                _ = isFields ? this.m_builder.Append(tableEntry.Key) : this.m_builder.Append("[\"").Append(tableEntry.Key).Append("\"]");
                WriteOperator('=');
            }
            if (tableEntry.Value is Table subTable) {
                WriteTableValue(subTable);
            } else if (tableEntry.Value is LuaLazyConverter lazyConverter) {
                lazyConverter.Convert();
            } else {
                this.WriteValue(tableEntry.Value);
            }
            if (i + 1 != entries.Length || i +1 == entries.Length && this.Options.WriteTrailingComma) {
                singleLine.IfTrue()
                        .Then(() => m_builder.Append(", "))
                        .Else(() => { _ = m_builder.Append(','); NewLine(); });
            }
        }
        if (singleLine)
            this.m_builder.Append(" }");
        else { 
            this.m_indent--; 
            NewLine(); 
            this.m_builder.Append('}'); 
        }
    }

    public string GetContent()
        => this.m_builder.ToString();

    public static bool IsLegalVariableName(string variable)
        => VarRegex.IsMatch(variable);

}
