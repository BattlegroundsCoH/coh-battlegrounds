using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Attribute to inform the <see cref="IJsonObject"/> serializer to ignore a field or property if it has a specific value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonIgnoreIfValueAttribute : Attribute {

        /// <summary>
        /// The value to not take in order to be serialized.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Simple <see cref="JsonIgnoreIfValueAttribute"/> attribute instantiation for a specific value.
        /// </summary>
        /// <param name="value">The value to ignore if empty.</param>
        public JsonIgnoreIfValueAttribute(object value) {
            this.Value = value;
        }

    }

}
