using System.Collections;
using System.Reflection;

using Battlegrounds.Extensions;

using YamlDotNet.Serialization.NamingConventions;

namespace Battlegrounds.Serializers;

/// <summary>
/// Provides functionality to deserialize a dictionary of key-value pairs into an instance of a specified type.
/// </summary>
/// <remarks>This class is designed to map the values in a <see cref="Dictionary{TKey, TValue}"/> to the
/// properties of a  specified type <typeparamref name="T"/>. The type <typeparamref name="T"/> must have a
/// parameterless constructor  and writable properties that match the keys in the dictionary.  The deserialization
/// process attempts to convert dictionary values to the target property types, including handling  enums and basic type
/// conversions. If a value cannot be converted, an <see cref="InvalidOperationException"/> is thrown.</remarks>
public class DictionaryDeserializer {

    private readonly Dictionary<Type, Func<string, object>> _typeConverters = new() {
        { typeof(Guid), s => Guid.Parse(s) },
        { typeof(DateTime), s => DateTime.Parse(s) },
    };

    private static readonly Dictionary<Type, PropertyInfo[]> _propertyCache = [];
    private static readonly Dictionary<Type, ConstructorInfo[]> _constructorCache = [];
    private static readonly Dictionary<Type, ConstructorInfo> _defaultConstructorCache = [];

    /// <summary>
    /// Creates an instance of the specified type <typeparamref name="T"/> and populates its properties using values
    /// from the provided dictionary.
    /// </summary>
    /// <remarks>This method attempts to map dictionary values to the properties of the specified type
    /// <typeparamref name="T"/>. If a property in <typeparamref name="T"/> cannot be written to or the value in the
    /// dictionary is null,  the property will remain unchanged.</remarks>
    /// <typeparam name="T">The type of the object to create. Must have a parameterless constructor.</typeparam>
    /// <param name="dictionary">A dictionary containing property names as keys and their corresponding values. Keys should match the property
    /// names of the type <typeparamref name="T"/>.</param>
    /// <returns>An instance of type <typeparamref name="T"/> with its writable properties set to the corresponding values from
    /// the dictionary. If the dictionary is null or empty, a new instance of <typeparamref name="T"/>  is returned with
    /// default property values.</returns>
    public T DeserializeFromDictionary<T>(Dictionary<string, object> dictionary) where T : new() {
        if (dictionary == null || dictionary.Count == 0) {
            return new T(); // Return a new instance if the dictionary is empty
        }
        T instance = new T();
        return (T)DeserializeFromDictionaryInternal(typeof(T), instance, dictionary);
    }

    /// <summary>
    /// Deserializes an object of the specified type from a dictionary of key-value pairs.
    /// </summary>
    /// <remarks>This method attempts to populate the object using the following approach: <list
    /// type="bullet"> <item> If the type has a parameterless constructor, it creates an instance using that constructor
    /// and populates its properties. </item> <item> If the type does not have a parameterless constructor, it uses the
    /// first available constructor and maps dictionary values to its parameters. </item> </list> Keys in the dictionary
    /// can use either the original property/parameter names or a hyphenated naming convention. Missing values in the
    /// dictionary are replaced with default values for the corresponding types.</remarks>
    /// <param name="type">The <see cref="Type"/> of the object to deserialize. Must be a class type with at least one constructor.</param>
    /// <param name="dictionary">A dictionary containing the key-value pairs used to populate the object's properties or constructor parameters.
    /// Keys should match property names or constructor parameter names.</param>
    /// <returns>An instance of the specified <paramref name="type"/> populated with values from the <paramref
    /// name="dictionary"/>. If the dictionary is null or empty, a new instance of the type is created using its
    /// parameterless constructor (if available).</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="type"/> does not have a parameterless constructor or any constructors at
    /// all. Also thrown if a value in the dictionary cannot be converted to the expected type of a constructor
    /// parameter.</exception>
    public object DeserializeFromDictionary(Type type, Dictionary<string, object> dictionary) {
        if (dictionary == null || dictionary.Count == 0) {
            return Activator.CreateInstance(type)!; // Return a new instance if the dictionary is empty
        }
        ConstructorInfo[] ctors = _constructorCache.GetOrCompute(type, _ => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        if (ctors.Any(c => c.GetParameters().Length == 0)) {
            // If there is a parameterless constructor, use it
            return DeserializeFromDictionaryInternal(type, Activator.CreateInstance(type)!, dictionary);
        }
        var ctor = _defaultConstructorCache.GetOrCompute(type, 
            _ => ctors.FirstOrDefault() ?? throw new InvalidOperationException($"Type '{type.FullName}' does not have a parameterless constructor or any constructors at all."));
        var parameters = ctor.GetParameters() ?? [];
        var args = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++) {
            if (dictionary.TryGetValue(parameters[i].Name ?? string.Empty, out var value)) {
                args[i] = MapType(value, parameters[i].ParameterType) ?? throw new InvalidOperationException($"Cannot convert value '{value}' to type '{parameters[i].ParameterType.FullName}'");
            } else if (dictionary.TryGetValue(HyphenatedNamingConvention.Instance.Apply(parameters[i].Name ?? string.Empty), out var otherValue)) { 
                args[i] = MapType(otherValue, parameters[i].ParameterType) ?? throw new InvalidOperationException($"Cannot convert value '{otherValue}' to type '{parameters[i].ParameterType.FullName}'");
            } else {
                args[i] = GetDefaultValue(parameters[i].ParameterType)!; // Default value for missing parameters
            }
        }
        var instance = ctor.Invoke(args);
        return DeserializeFromDictionaryInternal(type, instance!, dictionary);
    }

    private object DeserializeFromDictionaryInternal(Type type, object instance, Dictionary<string, object> dictionary) {
        var properties = _propertyCache.GetOrCompute(type, t => t.GetProperties());
        foreach (var property in properties) {
            if (dictionary.TryGetValue(property.Name, out var value)) {
                if (value is not null && property.CanWrite) {
                    property.SetValue(instance, MapType(value, property.PropertyType));
                }
            }
        }
        return instance!;
    }

    private object? MapType(object value, Type targetType) {
        if (value is null || targetType.IsInstanceOfType(value)) {
            return value;
        }
        if (targetType.IsEnum) {
            return Enum.Parse(targetType, value.ToString() ?? string.Empty, true);
        }
        return value switch {
            string str => _typeConverters.TryGetValue(targetType, out var converter) ? converter(str) : Convert.ChangeType(str, targetType),
            Dictionary<string, object> dict when targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>) =>
                DeserializeFromDictionary(targetType.GetGenericArguments()[1], dict),
            Dictionary<object, object> dict when targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>) =>
                DeserializeFromDictionary(targetType.GetGenericArguments()[1], dict.ToDictionary(k => k.Key.ToString()!, v => v.Value)),
            Dictionary<object, object> dict =>
                DeserializeFromDictionary(targetType, dict.ToDictionary(k => k.Key.ToString()!, v => v.Value)),
            ICollection<KeyValuePair<object, object>> kvpCollection when targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>) =>
                DeserializeFromDictionary(targetType.GetGenericArguments()[1], kvpCollection.ToDictionary(k => k.Key.ToString()!, v => v.Value)),
            double d when targetType == typeof(float) => (float)d, // Handle double to float conversion
            _ => TryHandleCollectionOrThrow(value, targetType)
        };
    }

    private object TryHandleCollectionOrThrow(object value, Type targetType) {
        if (value is IEnumerable enumerable) {
            var elementType = GetElementType(targetType);
            if (targetType.IsArray) {
                return TypesafeArray(enumerable.Cast<object>(), elementType);
            }
            return TypesafeList(enumerable.Cast<object>(), elementType); // For List<T> or IList<T>
        }
        return Convert.ChangeType(value, targetType) ?? throw new InvalidOperationException($"Cannot convert value '{value}' ({value.GetType().FullName!}) to type '{targetType.FullName}'");
    }

    private Array TypesafeArray(IEnumerable<object> items, Type elementType) {
        var array = Array.CreateInstance(elementType, items.Count());
        int index = 0;
        foreach (var item in items) {
            array.SetValue(MapType(item, elementType), index++);
        }
        return array;
    }

    private object TypesafeList(IEnumerable<object> items, Type elementType) {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var item in items) {
            list.Add(MapType(item, elementType));
        }
        return list;
    }

    private static Type GetElementType(Type type) {
        if (type.IsArray) {
            return type.GetElementType()!;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
            return type.GetGenericArguments()[0];
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>)) {
            return type.GetGenericArguments()[0];
        }
        return type;
    }

    /// <summary>
    /// Registers a type converter for converting string values to the specified type.
    /// </summary>
    /// <remarks>This method allows you to define custom conversion logic for a specific type. The registered
    /// converter will be used to transform string values into instances of the specified type.</remarks>
    /// <typeparam name="T">The type to which the string values will be converted. Must be a non-nullable type.</typeparam>
    /// <param name="converter">A function that converts a string to an instance of type <typeparamref name="T"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converter"/> is <see langword="null"/>.</exception>
    public void RegisterTypeConverter<T>(Func<string, T> converter) where T : notnull {
        if (converter == null) {
            throw new ArgumentNullException(nameof(converter), "Converter cannot be null.");
        }
        _typeConverters[typeof(T)] = s => converter(s);
    }

    private static object? GetDefaultValue(Type type) {
        if (type.IsValueType) {
            return Activator.CreateInstance(type);
        }
        if (type == typeof(string)) {
            return string.Empty;
        }
        return null; // For reference types, return null
    }

}
