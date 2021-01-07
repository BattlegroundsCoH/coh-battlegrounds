using System;

namespace Battlegrounds.Modding {
    
    /// <summary>
    /// Readonly representation of a <see cref="Guid"/> object to fit the Company of Heroes format.
    /// </summary>
    public readonly struct ModGuid {

        private readonly string m_guid;

        /// <summary>
        /// The fixed length of the company GUI
        /// </summary>
        public const byte FIXED_LENGTH = 32;

        /// <summary>
        /// Get the Company of Heroes <see cref="string"/> representation of the <see cref="Guid"/>.
        /// </summary>
        public string GUID => this.m_guid;

        private ModGuid(Guid guid) {
            this.m_guid = guid.ToString().Replace("-", "");
        }

        public override string ToString() => this.m_guid;

        public static implicit operator string(ModGuid guid) => guid.m_guid;

        public static implicit operator Guid(ModGuid guid) => new Guid(guid.m_guid);

        /// <summary>
        /// Convert a <see cref="Guid"/> instance into a Company of Heroes 2 mod GUID format compliant representation.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> instance to convert.</param>
        /// <returns>A formatted <see cref="ModGuid"/> instance.</returns>
        public static ModGuid FromGuid(Guid guid) => new ModGuid(guid);

    }

}
