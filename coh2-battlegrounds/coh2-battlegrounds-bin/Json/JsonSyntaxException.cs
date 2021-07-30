using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// <see cref="Exception"/> subtype representing a syntax-error in a json file.
    /// </summary>
    public class JsonSyntaxException : Exception {

        /// <summary>
        /// <see cref="JsonSyntaxException"/> instance with a custom message.
        /// </summary>
        /// <param name="message">The custom message to give when throwing the error.</param>
        public JsonSyntaxException(string message) : base(message) { }

    }

}
