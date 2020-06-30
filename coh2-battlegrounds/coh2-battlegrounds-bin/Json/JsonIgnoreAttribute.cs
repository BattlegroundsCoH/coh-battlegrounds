using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Tells the <see cref="IJsonObject"/> serializer to ignore this member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonIgnoreAttribute : Attribute {}

}
