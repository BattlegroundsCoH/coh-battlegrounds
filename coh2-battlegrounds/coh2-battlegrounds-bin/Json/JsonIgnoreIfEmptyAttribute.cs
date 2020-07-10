using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Attribute to inform the <see cref="IJsonObject"/> serialiser to ignore a field or property if empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonIgnoreIfEmptyAttribute : Attribute {}

}
