using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Battlegrounds.Functional;
using Battlegrounds.Lua.Generator.RuntimeServices;

namespace Battlegrounds.Lua.Generator {

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
            this.Options = new();
            this.m_builder = new();
            this.m_indent = 0;
            this.m_line = 0;
        }

        public void IncreaseIndent() => this.m_indent++;

        public void DecreaseIndent() => this.m_indent--;

        public void NewLine() {
            _ = this.m_builder.Append(this.Options.NewLine).Append('\t', this.m_indent);
            this.m_line++;
        }

        public void EndLine(bool ignoreSemicolonAnyways) {
            if (!ignoreSemicolonAnyways && this.Options.WriteSemicolon && !this.IsEmptyLine) {
                _ = this.m_builder.Append(';');
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
                this.NewLine(); // Will always end line                
            }
            if (keyword is LuaKw.For or LuaKw.Function or LuaKw.If or LuaKw.Else or LuaKw.ElseIf) {
                this.m_indent++; // Will always increase indent
            }
        }

        public void WriteOperator(char op) {
            switch (op) {
                case '=':
                case '+':
                case '-':
                case '*':
                case '/':
                case '^':
                    _ = this.m_builder.Append(' ').Append(op).Append(' ');
                    break;
                case ',':
                    _ = this.m_builder.Append(", ");
                    break;
                case '(':
                case ')':
                    _ = this.m_builder.Append(op);
                    break;
                default:
                    throw new InvalidOperationException($"'{op}' is not a valid operator.");
            }
        }

        public void WriteVerbatim(string verbatim) => this.m_builder.Append(verbatim);

        public void WriteVariable(string variable)
            => this.m_builder.Append(variable);

        public void WriteVariableAssignment(string variable) {
            _ = this.m_builder.Append(variable);
            this.WriteOperator('=');
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

        public void WriteValue(LuaValue value) {
            switch (value) {
                case LuaNil:
                    this.WriteNilValue();
                    break;
                case LuaString lstr:
                    this.WriteStringValue(lstr.Str());
                    break;
                case LuaNumber lnum:
                    if (lnum.IsInteger()) {
                        this.WriteNumberValue(lnum.ToInt());
                    } else {
                        this.WriteNumberValue(lnum);
                    }
                    break;
                case LuaBool lbool:
                    this.WriteBooleanValue(lbool.IsTrue);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void WriteValue(object value) {
            switch (value) {
                case LuaValue luaValue:
                    this.WriteValue(luaValue);
                    break;
                case string s:
                    this.WriteStringValue(s);
                    break;
                case byte ui8:
                    this.WriteNumberValue(ui8);
                    break;
                case sbyte i8:
                    this.WriteNumberValue(i8);
                    break;
                case short i16:
                    this.WriteNumberValue(i16);
                    break;
                case int i32:
                    this.WriteNumberValue(i32);
                    break;
                case long i64:
                    this.WriteNumberValue(i64);
                    break;
                case ushort ui16:
                    this.WriteNumberValue(ui16);
                    break;
                case uint ui32:
                    this.WriteNumberValue(ui32);
                    break;
                case ulong ui64:
                    this.WriteNumberValue(ui64);
                    break;
                case float s32:
                    this.WriteNumberValue(s32);
                    break;
                case double s64:
                    this.WriteNumberValue(s64);
                    break;
                case bool b:
                    this.WriteBooleanValue(b);
                    break;
                case char c:
                    this.WriteStringValue(c.ToString());
                    break;
                default:
                    this.WriteNilValue();
                    break;
            }
        }

        private int CalculateLengthOfTableEntries(LuaTable table) {
            int sum = 0;
            foreach (var tableEntry in table) {
                sum += tableEntry.Key.Str().Length + 7;
                if (tableEntry.Value is LuaTable subTable) {
                    sum += this.CalculateLengthOfTableEntries(subTable);
                } else {
                    sum += tableEntry.Value.Str().Length;
                }
                sum++; // for the potential comma
                if (sum > this.Options.SingleLineTableLength) {
                    return sum + this.Options.SingleLineTableLength;
                }
            }
            return sum;
        }

        public void WriteTableValue(LuaTable table) {
            if (table.Size is 0) {
                _ = this.m_builder.Append("{}");
                return;
            }
            bool isArray = table.IsArray();
            int pseudoSize = (this.m_indent * 4) + this.CalculateLengthOfTableEntries(table);
            bool singleLine = pseudoSize <= this.Options.SingleLineTableLength;
            bool isFields = !isArray && table.StringKeys.All(IsLegalVariableName);
            _ = this.m_builder.Append('{').IfFalse(_ => singleLine)
                .Then(q => { this.m_indent++; this.NewLine(); })
                .Else(q => q.Append(' '));
            var last = table.LastKey;
            foreach (var tableEntry in table) {
                if (!isArray) {
                    _ = isFields ? this.m_builder.Append(tableEntry.Key.Str()) : this.m_builder.Append("[\"").Append(tableEntry.Key.Str()).Append("\"]");
                    this.WriteOperator('=');
                }
                if (tableEntry.Value is LuaTable subTable) {
                    this.WriteTableValue(subTable);
                } else if (tableEntry.Value is LuaUserObject userObj && userObj.Object is LuaLazyConverter lazyConverter) {
                    lazyConverter.Convert();
                } else {
                    this.WriteValue(LuaMarshal.FromLuaValue(tableEntry.Value));
                }
                if (tableEntry.Key != last || (tableEntry.Key == last && this.Options.WriteTrailingComma)) {
                    singleLine.IfTrue()
                            .Then(() => this.m_builder.Append(", "))
                            .Else(() => { _ = this.m_builder.Append(','); this.NewLine(); });
                }
            }
            singleLine.IfTrue()
                .Then(() => this.m_builder.Append(" }"))
                .Else(() => { this.m_indent--; this.NewLine(); _ = this.m_builder.Append('}'); });
        }

        public string GetContent()
            => this.m_builder.ToString();

        public static bool IsLegalVariableName(string variable)
            => VarRegex.IsMatch(variable);

    }

}
