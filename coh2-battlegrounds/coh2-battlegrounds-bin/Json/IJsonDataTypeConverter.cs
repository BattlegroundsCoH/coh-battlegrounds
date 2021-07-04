namespace Battlegrounds.Json {

    /// <summary>
    /// Interface for a json data type converter.
    /// </summary>
    /// <typeparam name="T">The type to convert to and from its json <see cref="string"/> representation.</typeparam>
    public interface IJsonDataTypeConverter<T> {

        /// <summary>
        /// Convert a <see cref="string"/> into the expected <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="stringValue">The <see cref="string"/> value to convert.</param>
        /// <returns>The <typeparamref name="T"/> object that could be converted from the <see cref="string"/> representation.</returns>
        T ConvertFromString(string stringValue);

        /// <summary>
        /// Convert a <typeparamref name="T"/> into its <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">The <typeparamref name="T"/> object to convert.</param>
        /// <returns>A <see cref="string"/> representation of the <typeparamref name="T"/> object.</returns>
        string ConvertToString(T value);

    }

}
