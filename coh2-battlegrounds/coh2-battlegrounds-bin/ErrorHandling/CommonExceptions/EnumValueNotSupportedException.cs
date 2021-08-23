using System;

namespace Battlegrounds.ErrorHandling.CommonExceptions {

    /// <summary>
    /// Exception throw when an unsupported enum composition is encountered.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public class EnumValueNotSupportedException<T> : BattlegroundsException where T : Enum {
    
        /// <summary>
        /// Get the invalid enum value encountered. May be default if none is given.
        /// </summary>
        public Enum InvalidComposition { get; }

        /// <summary>
        /// Initialise a new <see cref="EnumValueNotSupportedException{T}"/> instance with no additional details.
        /// </summary>
        public EnumValueNotSupportedException() : base($"Encountered invalid enum value of type '{typeof(T).FullName}'.")
            => this.InvalidComposition = default(T);

        /// <summary>
        /// Initialise a new <see cref="EnumValueNotSupportedException{T}"/> instance with a specific <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message to display in the exception.</param>
        public EnumValueNotSupportedException(string message) : base(message)
            => this.InvalidComposition = default(T);

        /// <summary>
        /// Initialise a new <see cref="EnumValueNotSupportedException{T}"/> instance with a specific <typeparamref name="T"/> value that caused the exception.
        /// </summary>
        /// <param name="enumValue">The value that triggered the exception.</param>
        public EnumValueNotSupportedException(T enumValue) : this($"Encountered invalid enum value '{enumValue}'.")
            => this.InvalidComposition = enumValue;

    }

}
