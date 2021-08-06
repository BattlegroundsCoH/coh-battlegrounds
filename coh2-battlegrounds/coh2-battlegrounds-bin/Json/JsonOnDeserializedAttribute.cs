using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Json attribute marking method to be called once a deserialize operation is over.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class JsonOnDeserializedAttribute : Attribute { }

}
