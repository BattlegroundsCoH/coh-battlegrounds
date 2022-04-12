using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoH2XML2JSON {

    public class StringEnumConverter : JsonConverterFactory {

        public class ConcreteConverter<T> : JsonConverter<T> {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
        }

        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) 
            => Activator.CreateInstance(typeof(ConcreteConverter<>).MakeGenericType(typeToConvert)) as JsonConverter;

    }

}
