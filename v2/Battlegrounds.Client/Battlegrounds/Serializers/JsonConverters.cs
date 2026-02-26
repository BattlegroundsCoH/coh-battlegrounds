using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Serializers;

/// <summary>
/// Converts <see cref="IReadOnlySet{T}"/> to and from JSON by using <see cref="HashSet{T}"/> as the concrete type.
/// </summary>
public sealed class ReadOnlySetConverterFactory : JsonConverterFactory {

    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlySet<>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter?)Activator.CreateInstance(typeof(ReadOnlySetConverter<>).MakeGenericType(elementType));
    }

    private sealed class ReadOnlySetConverter<T> : JsonConverter<IReadOnlySet<T>> {
        public override IReadOnlySet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<HashSet<T>>(ref reader, options) ?? [];

        public override void Write(Utf8JsonWriter writer, IReadOnlySet<T> value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            foreach (T item in value)
                JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }
    }

}

/// <summary>
/// Converts <see cref="LinkedList{T}"/> to and from JSON arrays.
/// </summary>
public sealed class LinkedListConverterFactory : JsonConverterFactory {

    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(LinkedList<>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter?)Activator.CreateInstance(typeof(LinkedListConverter<>).MakeGenericType(elementType));
    }

    private sealed class LinkedListConverter<T> : JsonConverter<LinkedList<T>> {
        public override LinkedList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var items = JsonSerializer.Deserialize<List<T>>(ref reader, options);
            return items is null ? [] : new LinkedList<T>(items);
        }

        public override void Write(Utf8JsonWriter writer, LinkedList<T> value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            foreach (T item in value)
                JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }
    }

}
