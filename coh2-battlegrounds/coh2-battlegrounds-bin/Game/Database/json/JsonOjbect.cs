using System;

namespace Battlegrounds.Game.Database.json {
    
    /// <summary>
    /// Json interface for converting object to and from a Json object
    /// </summary>
    public interface IJsonOjbect {

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
        public static T Deserialize<T>(string json) where T : IJsonOjbect {

            T t = Activator.CreateInstance<T>();

            return t;

        }

    }

}
