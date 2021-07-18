using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// Readonly struct that epresents a unique identifier for a blueprint, by mod.
    /// </summary>
    public readonly struct BlueprintUID {

        /// <summary>
        /// The <see cref="ModGuid"/> associated with this blueprint UID
        /// </summary>
        public readonly ModGuid Mod;

        /// <summary>
        /// The unique identifier associated with the blueprint.
        /// </summary>
        public readonly ulong UniqueIdentifier;

        /// <summary>
        /// Initialise a new <see cref="BlueprintUID"/> instance with <paramref name="uid"/> for a <see cref="ModGuid.BaseGame"/> blueprint.
        /// </summary>
        /// <param name="uid">The unique ID.</param>
        public BlueprintUID(ulong uid) : this(uid, ModGuid.BaseGame) {}

        /// <summary>
        /// Initialise a new <see cref="BlueprintUID"/> instance with <paramref name="uid"/> for a <paramref name="mod"/> blueprint.
        /// </summary>
        /// <param name="uid">The unique ID.</param>
        /// <param name="mod">The mod to associate <paramref name="uid"/> with.</param>
        public BlueprintUID(ulong uid, ModGuid mod) {
            this.UniqueIdentifier = uid;
            this.Mod = mod;
        }

        public override string ToString() => this.Mod == ModGuid.BaseGame ? this.UniqueIdentifier.ToString() : $"{this.Mod.GUID}:{this.UniqueIdentifier}";

        public override bool Equals(object obj) => obj is BlueprintUID bip && bip.UniqueIdentifier == this.UniqueIdentifier && bip.Mod.Equals(this.Mod);

        public override int GetHashCode() {
            HashCode code = new();
            code.Add(this.Mod);
            code.Add(this.UniqueIdentifier);
            return code.ToHashCode();
        }

        public static bool operator ==(BlueprintUID left, BlueprintUID right) => left.Equals(right);

        public static bool operator !=(BlueprintUID left, BlueprintUID right) => !(left == right);
    
    }

}
