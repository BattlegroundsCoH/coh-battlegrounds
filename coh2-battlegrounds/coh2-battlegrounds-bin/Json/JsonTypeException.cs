using System;

namespace Battlegrounds.Json {

    /// <summary>
    /// Represents an error related to a type conversion to or from Json.
    /// </summary>
    public class JsonTypeException : Exception {
        
        /// <summary>
        /// The <see cref="Type"/> that caused the <see cref="JsonTypeException"/>.
        /// </summary>
        public Type ErrorType { get; }

        public JsonTypeException(Type type) : base($"Invalid type for Json use '{type}'.") {
            this.ErrorType = type;
        }
        
        public JsonTypeException(Type type, string message) : base(message) {
            this.ErrorType = type;
        }

    }

}
