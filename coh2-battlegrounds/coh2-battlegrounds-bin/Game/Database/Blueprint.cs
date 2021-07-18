using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Scar;
using Battlegrounds.Json;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// Represents a <see cref="Blueprint"/> for the behaviour of instances within Company of Heroes 2. Implements <see cref="IJsonObject"/>. Implements <see cref="IScarValue"/>.
    /// </summary>
    public class Blueprint : IJsonObject, IScarValue {
    
        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public ulong PBGID { get; set; }

        /// <summary>
        /// The unique PropertyBagGroupID assigned to this blueprint at load-time.
        /// </summary>
        public ushort ModPBGID { get; set; }

        /// <summary>
        /// The name of the <see cref="Blueprint"/> file in the game files (See the instances folder in the mod tools).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of <see cref="Blueprint"/>.
        /// </summary>
        public BlueprintType BlueprintType { get; set; }

        /// <summary>
        /// The unique mod GUID associated with this <see cref="Blueprint"/>. Empty if vanilla blueprint.
        /// </summary>
        public string ModGUID { get; set; }

        /// <summary>
        /// Convert the <see cref="Blueprint"/> to its Json reference value.
        /// </summary>
        /// <returns>The string representation of the <see cref="Blueprint"/> json reference value.</returns>
        public string ToJsonReference() => this.ToString();

        public virtual string ToScar() { 
            if (this.ModGUID.CompareTo(string.Empty) == 0) {
                return $"\"{this.Name}\"";
            } else {
                return $"\"{this.ModGUID.Replace("-", "")}:{this.Name}\"";
            }
        }

        /// <summary>
        /// Get the <see cref="BlueprintUID"/> associated with this <see cref="Blueprint"/> instance.
        /// </summary>
        /// <returns>The <see cref="BlueprintUID"/> identifying the <see cref="Blueprint"/>.</returns>
        public virtual BlueprintUID GetBlueprintUID() => new(this.PBGID, ModGuid.FromGuid(this.ModGUID));

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{this.BlueprintType}:{this.Name}";

        public override bool Equals(object obj) { 
            if (obj is Blueprint bp && this != null) {
                return bp.BlueprintType == this.BlueprintType && bp.PBGID == this.PBGID && bp.ModGUID.CompareTo(this.ModGUID) == 0;
            } else {
                return false;
            }
        }

        public override int GetHashCode() => this.ModGUID.GetHashCode() + this.ModPBGID;

    }

}
