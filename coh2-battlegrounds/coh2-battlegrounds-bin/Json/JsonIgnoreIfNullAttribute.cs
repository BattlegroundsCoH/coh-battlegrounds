using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Attribute informing the <see cref="IJsonObject"/> serializer to ignore this field if it's null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonIgnoreIfNullAttribute : Attribute {}

}
