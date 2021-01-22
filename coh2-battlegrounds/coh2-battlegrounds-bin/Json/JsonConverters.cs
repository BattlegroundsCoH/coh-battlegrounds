using System;
using System.Collections.Generic;
using Battlegrounds.Json.DataConverters;

namespace Battlegrounds.Json {
    
    /// <summary>
    /// Static helper class for Json data type converters
    /// </summary>
    public static class JsonConverters {

        private static Dictionary<Type, object> __Converters;

        static JsonConverters() {
            __Converters = new Dictionary<Type, object> {
                { typeof(TimeSpan), new TimespanConverter() }
            };
        }

        /// <summary>
        /// Check if the given <see cref="Type"/> has a <see cref="IJsonDataTypeConverter{T}"/> associated with it.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check converter for.</param>
        /// <returns>Will return <see langword="true"/> if there's a converter available. Otherwise <see langword="false"/>.</returns>
        public static bool HasConverter(Type type) => __Converters.ContainsKey(type);

        /// <summary>
        /// Register a new <see cref="IJsonDataTypeConverter{T}"/> converter.
        /// </summary>
        /// <typeparam name="T">Datatype to convert</typeparam>
        /// <param name="converter">The converter object to register.</param>
        /// <returns>Will return <see langword="true"/> if the onverter was registered. Otherwise <see langword="false"/>.</returns>
        public static bool Register<T>(IJsonDataTypeConverter<T> converter) => __Converters.TryAdd(typeof(T), converter);

        /// <summary>
        /// Will get the converter for the given <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// Make sure to verify there's a converter before calling this method.
        /// </remarks>
        /// <typeparam name="T">Datatype to get converter for.</typeparam>
        /// <returns>Will return the <see cref="IJsonDataTypeConverter{T}"/> converter associated with <typeparamref name="T"/>.</returns>
        public static IJsonDataTypeConverter<T> GetConverter<T>() => __Converters[typeof(T)] as IJsonDataTypeConverter<T>;

        /// <summary>
        /// Will get the converter associated with the <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// Make sure to verify there's a converter before calling this method. Otherwise runtime errors will happen.
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to find converter for.</param>
        /// <returns>Returns a <see langword="dynamic"/> that represents the associated <see cref="IJsonDataTypeConverter{T}"/>.</returns>
        public static dynamic GetConverter(Type type) => __Converters[type];

    }

}
