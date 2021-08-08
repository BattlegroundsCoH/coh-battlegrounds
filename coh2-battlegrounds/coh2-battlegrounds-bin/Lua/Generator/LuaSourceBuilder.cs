using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Battlegrounds.Functional;
using Battlegrounds.Util;

namespace Battlegrounds.Lua.Generator {

    /// <summary>
    /// Class representing a complex lua source code builder. This class cannot be inherited.
    /// </summary>
    public sealed class LuaSourceBuilder {

        private readonly LuaSourceWriter m_writer;

        public LuaSourceBuilderOptions Options {
            get => this.m_writer.Options;
            set => this.m_writer.Options = value;
        }

        public LuaSourceBuilder() => this.m_writer = new();

        public LuaSourceBuilder WriteAssignment(string variable, object value) {
            this.m_writer.WriteVariableAssignment(variable);
            switch (value) {
                case Array a:
                    this.m_writer.WriteTableValue(this.BuildTable(a));
                    break;
                case IDictionary dic:
                    this.m_writer.WriteTableValue(this.BuildTable(dic));
                    break;
                case ICollection col:
                    this.m_writer.WriteTableValue(this.BuildTable(col));
                    break;
                case string str:
                    this.m_writer.WriteStringValue(str);
                    break;
                case ValueType:
                    this.m_writer.WriteValue(value);
                    break;
                case object o:
                    this.m_writer.WriteTableValue(this.BuildTable(o));
                    break;
                default:
                    this.m_writer.WriteNilValue();
                    break;
            }
            this.m_writer.EndLine();
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

        public string GetSourceTest() => this.m_writer.GetContent();

    }

}
