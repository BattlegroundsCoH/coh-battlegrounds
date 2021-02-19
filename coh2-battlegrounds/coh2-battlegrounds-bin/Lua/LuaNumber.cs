using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Lua {

    public class LuaNumber : LuaValue {

        public static readonly CultureInfo NumberCulture = CultureInfo.GetCultureInfo("en-GB");

        private double m_number;
        private bool m_treatAsInteger;

        public LuaNumber(double value) {
            this.m_number = value;
            this.m_treatAsInteger = false;
        }

        public LuaNumber(int value) {
            this.m_number = value;
            this.m_treatAsInteger = true;
        }

        public LuaNumber AsInteger() => new LuaNumber((int)this.m_number);

        public bool IsInteger() => (this.m_number % 1) <= double.Epsilon * 100;

        public override bool Equals(LuaValue value) => value is LuaNumber n && n.m_number == this.m_number;

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        public override string Str() => this.m_number.ToString();

        public override int GetHashCode() => this.m_number.GetHashCode() ^ this.m_treatAsInteger.GetHashCode();

        public override string ToString() => this.m_treatAsInteger ? ((int)this.m_number).ToString() : this.m_number.ToString(NumberCulture);

        public static explicit operator int(LuaNumber n) => n.IsInteger() ? (int)n.m_number : throw new InvalidCastException("Cannot cast non-integer LuaNumber to an integer");

        public static implicit operator double(LuaNumber n) => n.m_number;

    }

}
