using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Json attribute helping the <see cref="JsonParser"/> to deserialize objects implementing <see cref="IJsonObject"/> using enums.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonEnumAttribute : Attribute {

        private Type m_enumType;
        private string[] m_values;

        /// <summary>
        /// The accepted values by the attribute
        /// </summary>
        public string[] Values => m_values;

        /// <summary>
        /// The .Net enum type
        /// </summary>
        public Type EnumType => m_enumType;

        /// <summary>
        /// Does the attribute have a specific .NET enum type
        /// </summary>
        public bool IsNetEnum => m_enumType != null;

        /// <summary>
        /// Json enum attribute with specific .NET enum type.
        /// </summary>
        /// <param name="enumType">The .NET enum type to use.</param>
        public JsonEnumAttribute(Type enumType) {
            m_enumType = enumType;
            m_values = Enum.GetNames(enumType);
        }

        /// <summary>
        /// Json enum attribute with unspecified .NET enum type but specific set of allowed values.
        /// </summary>
        /// <param name="allowedValues">The allowed values in the Json enum.</param>
        public JsonEnumAttribute(params string[] allowedValues) {
            m_enumType = null;
            m_values = allowedValues;
        }

    }

}
