using System.Collections.Generic;
using System.Text.Json;

namespace Battlegrounds.Functional {

    public static class FunctionJsonReader {

        public static string ReadProperty(this ref Utf8JsonReader reader) {
            string val = reader.GetString();
            reader.Read();
            return val;
        }

        public static float ReadNumberProperty(this ref Utf8JsonReader reader) {
            float val = reader.GetSingle();
            reader.Read();
            return val;
        }

        public static bool ReadBoolProperty(this ref Utf8JsonReader reader) {
            bool val = reader.GetBoolean();
            reader.Read();
            return val;
        }

        public static List<string> GetStringList(this ref Utf8JsonReader reader) {
            List<string> values = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
                values.Add(reader.GetString());
            }
            return values;
        }

        public static string[] GetStringArray(this ref Utf8JsonReader reader)
            => reader.GetStringList().ToArray();

    }

}
