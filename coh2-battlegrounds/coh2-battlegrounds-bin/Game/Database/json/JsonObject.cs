using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Battlegrounds.Util;

namespace Battlegrounds.Game.Database.json {
    
    /// <summary>
    /// Json interface for converting object to and from a Json object. Implements <see cref="IJsonElement"/>.
    /// </summary>
    public interface IJsonObject : IJsonElement {

        /// <summary>
        /// Serialize self into a json object
        /// </summary>
        /// <returns></returns>
        public virtual string Serialize() {

            Type il_type = this.GetType();
            TxtBuilder jsonbuilder = new TxtBuilder();

            // Get all the fields
            IEnumerable<FieldInfo> fields = il_type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() is null && !x.Name.Contains("_BackingField"));

            // Get all the properties
            IEnumerable<PropertyInfo> properties = il_type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() is null);

            jsonbuilder.AppendLine("{");
            jsonbuilder.IncreaseIndent();

            jsonbuilder.AppendLine($"\"jsdb-type\": \"{il_type.FullName}\"{((fields.Count() + properties.Count() > 0) ? "," : "")}");

            foreach (FieldInfo finfo in fields) {
                WriteKeyValuePair(jsonbuilder, finfo.FieldType, finfo.Name, finfo.GetValue(this), finfo != fields.Last() || properties.Count() > 0);
            }

            foreach (PropertyInfo pinfo in properties) {
                WriteKeyValuePair(jsonbuilder, pinfo.PropertyType, pinfo.Name, pinfo.GetValue(this), pinfo != properties.Last());
            }

            jsonbuilder.DecreaseIndent();
            jsonbuilder.AppendLine("}");

            return jsonbuilder.GetContent();

        }

        private static void WriteKeyValuePair(TxtBuilder jsonbuilder, Type type, string name, object val, bool appendComma) {
            if (type.IsPrimitive || val is string) {
                jsonbuilder.AppendLine($"\"{name}\": \"{val}\"{((appendComma)?",":"")}");
            } else {
                if (type.IsAssignableFrom(typeof(IJsonObject))) {

                } else if (type.GenericTypeArguments.Length > 0 && type.GetInterfaces().Contains(typeof(IEnumerable<>).MakeGenericType(type.GenericTypeArguments))) {
                    jsonbuilder.AppendLine($"\"{name}\": [");
                    dynamic dynVal = val; // CAREFUL!
                    foreach (dynamic c in dynVal) {
                        Type t = c.GetType();
                        if (t.IsPrimitive || c is string) {
                            Console.WriteLine();
                        } else if (c is IJsonObject jso) {
                            string str = jso.Serialize();
                            Console.WriteLine();
                        }
                    }
                    jsonbuilder.AppendLine($"]{((appendComma)?", ":"")}");
                } else {
                    jsonbuilder.AppendLine($"\"{name}\": \"{val}\"{((appendComma) ? "," : "")}");
                }

            }
        }

        /// <summary>
        /// Derserialize a json string representing an object into the C# requivalent.
        /// </summary>
        /// <typeparam name="T">The object type to deserialize to</typeparam>
        /// <param name="json">The input string to deserialize</param>
        /// <returns>A deserialized instance of the json input string</returns>
        public static T Deserialize<T>(string json) where T : IJsonObject {

            T t = Activator.CreateInstance<T>();

            return t;

        }

        /// <summary>
        /// Derserialize a json string representing an object into the C# requivalent.
        /// </summary>
        /// <param name="parsedJson">The parsed input of a json object</param>
        /// <returns>A deserialized instance of the json input string</returns>
        internal static object Deserialize(Dictionary<string, object> parsedJson) {

            object type;
            if (!parsedJson.TryGetValue("jsdb-type", out type)) {
                throw new ArgumentException();
            } else {
                parsedJson.Remove("jsdb-type");
            }

            // Get the type
            Type il_type = Type.GetType(type as string);

            // Create object
            object source = Activator.CreateInstance(il_type);

            // Get all the fields
            FieldInfo[] fields = il_type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            // Get all the properties
            PropertyInfo[] properties = il_type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Set the value (fields expected!)
            foreach (var pair in parsedJson) {

                if (fields.FirstOrDefault(x => x.Name == pair.Key) is FieldInfo finfo) { // Is Field?

                    // Set value
                    finfo.SetValue(source, Convert.ChangeType(pair.Value, finfo.FieldType));

                } else if (properties.FirstOrDefault(x => x.Name == pair.Key) is PropertyInfo pinfo) { // Is property?

                    // Set value
                    pinfo.SetValue(source, Convert.ChangeType(pair.Value, pinfo.PropertyType));

                }

            }

            return source;

        }

    }

}
