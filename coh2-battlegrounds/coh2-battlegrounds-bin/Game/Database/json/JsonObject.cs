using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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

            string json = string.Empty;

            return json;

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
