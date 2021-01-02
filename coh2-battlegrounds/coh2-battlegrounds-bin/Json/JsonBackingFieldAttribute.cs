using System;

namespace Battlegrounds.Json {

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonBackingFieldAttribute : Attribute {
        public string Field { get; }
        public JsonBackingFieldAttribute(string fieldname) {
            this.Field = fieldname;
        }
    }

}
