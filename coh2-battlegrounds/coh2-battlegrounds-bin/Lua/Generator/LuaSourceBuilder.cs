using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Battlegrounds.Lua.Generator {

    /// <summary>
    /// Class representing a complex lua source code builder. This class cannot be inherited.
    /// </summary>
    public sealed class LuaSourceBuilder {

        public LuaSourceWriter Writer { get; }

        public LuaSourceBuilderOptions Options {
            get => this.Writer.Options;
            set => this.Writer.Options = value;
        }

        public LuaSourceBuilder() => this.Writer = new();

        public LuaSourceBuilder WriteAssignment(string variable, object value) {
            this.Writer.WriteVariableAssignment(variable);
            switch (value) {
                case Array a:
                    this.Writer.WriteTableValue(this.BuildTable(a));
                    break;
                case IDictionary dic:
                    this.Writer.WriteTableValue(this.BuildTable(dic));
                    break;
                case ICollection col:
                    this.Writer.WriteTableValue(this.BuildTable(col));
                    break;
                case string str:
                    this.Writer.WriteStringValue(str);
                    break;
                case ValueType:
                    this.Writer.WriteValue(value);
                    break;
                case object o:
                    this.Writer.WriteTableValue(this.BuildTable(o));
                    break;
                default:
                    this.Writer.WriteNilValue();
                    break;
            }
            this.Writer.EndLine(false);
            return this;
        }

        public LuaSourceBuilder WriteFunction(string name, params string[] args) {
            if (this.Writer.Line > 0) {
                this.Writer.EndLine(true);
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

        public LuaSourceBuilder Return() {
            this.Writer.WriteKeyword(LuaKw.Return);
            return this;
        }

        public LuaSourceBuilder End() {
            this.Writer.DecreaseIndent();
            if (!this.Writer.IsEmptyLine) {
                this.Writer.EndLine(false);
            }
            this.Writer.WriteKeyword(LuaKw.End);
            return this;
        }

        public LuaSourceBuilder Variables(params string[] vars) {
            for (int i = 0; i < vars.Length; i++) {
                this.Writer.WriteVerbatim(vars[i]);
                if (i < vars.Length - 1) {
                    this.Writer.WriteOperator(',');
                }
            }
            return this;
        }

        public LuaSourceBuilder Arithmetic(string left, char arithmeticOperation, string right) {
            this.Writer.WriteVerbatim(left);
            this.Writer.WriteOperator(arithmeticOperation);
            this.Writer.WriteVerbatim(right);
            return this;
        }

        public LuaSourceBuilder Arithmetic(double left, char arithmeticOperation, string right) {
            this.Writer.WriteNumberValue(left);
            this.Writer.WriteOperator(arithmeticOperation);
            this.Writer.WriteVerbatim(right);
            return this;
        }

        public LuaSourceBuilder Arithmetic(string left, char arithmeticOperation, double right) {
            this.Writer.WriteVerbatim(left);
            this.Writer.WriteOperator(arithmeticOperation);
            this.Writer.WriteNumberValue(right);
            return this;
        }

        public LuaSourceBuilder Arithmetic(double left, char arithmeticOperation, double right) {
            this.Writer.WriteNumberValue(left);
            this.Writer.WriteOperator(arithmeticOperation);
            this.Writer.WriteNumberValue(right);
            return this;
        }

        public LuaSourceBuilder Arithmetic(char arithmeticOperation, string right) {
            this.Writer.WriteOperator(arithmeticOperation);
            this.Writer.WriteVerbatim(right);
            return this;
        }

        public LuaSourceBuilder Arithmetic(char arithmeticOperation, double right) {
            this.Writer.WriteOperator(arithmeticOperation);
            this.Writer.WriteNumberValue(right);
            return this;
        }

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

        private LuaTable BuildTable(ICollection collection) {

            // Create table
            LuaTable table = new();

            // Return result
            return table;

        }

        private LuaTable BuildTable(Array array) {

            // Create table
            LuaTable table = new();

            // Return result
            return table;

        }

        private LuaTable BuildTable(IDictionary dictionary) {

            // Create table
            LuaTable table = new();

            // Loop over entries
            foreach (DictionaryEntry entry in dictionary) {
                if (entry.Value is null && this.Options.ExplicitNullAsNilValues) {
                    table[this.GetKeyName(entry.Key)] = LuaNil.Nil;
                } else if (entry.Value is not null) {
                    table[this.GetKeyName(entry.Key)] = LuaMarshal.ToLuaValue(entry.Value);
                }
            }

            // Return result
            return table;

        }

        private LuaValue GetTableValue(object value) => value switch {
            Array array => this.BuildTable(array),
            IDictionary dictionary => this.BuildTable(dictionary),
            ICollection collection => this.BuildTable(collection),
            null when this.Options.ExplicitNullAsNilValues => LuaNil.Nil,
            not null => LuaMarshal.ToLuaValue(value),
            _ => null
        };

        private LuaTable BuildTable(object obj) {

            // Create table
            LuaTable table = new();

            // Yay reflection...
            var publicProperties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Add Properties
            foreach (var property in publicProperties) {
                object tableValue = property.GetValue(obj);
                if (this.GetTableValue(tableValue) is LuaValue val) {
                    table[property.Name] = val;
                }
            }

            // Return result
            return table;

        }

        public string GetSourceTest() => this.Writer.GetContent();

    }

}
