using System;

namespace Battlegrounds.Modding {

    /// <summary>
    /// Readonly representation of a <see cref="Guid"/> object to fit the Company of Heroes format.
    /// </summary>
    public readonly struct ModGuid {

        /// <summary>
        /// The GUID representing the base-game.
        /// </summary>
        public static readonly ModGuid BaseGame = new(Guid.Empty);

        private readonly string m_guid;

        /// <summary>
        /// The fixed length of the GUID string
        /// </summary>
        public const byte FIXED_LENGTH = 32;

        /// <summary>
        /// Get the Company of Heroes <see cref="string"/> representation of the <see cref="Guid"/>.
        /// </summary>
        public string GUID => this.m_guid;

        /// <summary>
        /// Get if the given Mod GUID is valid (Has fixed length)
        /// </summary>
        public bool IsValid => this.GUID.Length == FIXED_LENGTH;

        private ModGuid(Guid guid) => this.m_guid = guid.ToString().Replace("-", "");

        private ModGuid(string guid) => this.m_guid = guid.Replace("-", "");

        public override string ToString() => this.m_guid;

        public static implicit operator string(ModGuid guid) => guid.m_guid;

        public static implicit operator Guid(ModGuid guid) => new Guid(guid.m_guid);

        public override bool Equals(object obj) => obj is ModGuid guid && guid.m_guid == this.m_guid;

        public static bool operator ==(ModGuid left, ModGuid right) => left.Equals(right);

        public static bool operator !=(ModGuid left, ModGuid right) => !(left == right);

        public override int GetHashCode() {
            HashCode code = new();
            code.Add(this.m_guid);
            return code.ToHashCode();
        }

        /// <summary>
        /// Convert a <see cref="Guid"/> instance into a Company of Heroes 2 mod GUID format compliant representation.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> instance to convert.</param>
        /// <returns>A formatted <see cref="ModGuid"/> instance.</returns>
        public static ModGuid FromGuid(Guid guid) => new ModGuid(guid);

        /// <summary>
        /// Convert a string into a Company of Heroes 2 mod GUID format compliant representation.
        /// </summary>
        /// <param name="guid">The string instance to convert</param>
        /// <returns>A formatted <see cref="ModGuid"/> instance.</returns>
        public static ModGuid FromGuid(string guid) => new ModGuid((guid ?? string.Empty).Replace("-", ""));

    }

}
