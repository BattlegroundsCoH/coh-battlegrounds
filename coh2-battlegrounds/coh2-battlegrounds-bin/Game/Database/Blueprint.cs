using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Scar;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// Represents a <see cref="Blueprint"/> for the behaviour of instances within Company of Heroes 2. Implements <see cref="IScarValue"/>.
    /// </summary>
    public abstract class Blueprint : IScarValue {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public abstract BlueprintUID PBGID { get; }

        /// <summary>
        /// The unique PropertyBagGroupID assigned to this blueprint at load-time.
        /// </summary>
        public ushort ModPBGID { get; set; }

        /// <summary>
        /// The name of the <see cref="Blueprint"/> file in the game files (See the instances folder in the mod tools).
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The type of <see cref="Blueprint"/>.
        /// </summary>
        public abstract BlueprintType BlueprintType { get; }

        public virtual string ToScar() {
            if (string.IsNullOrEmpty(this.PBGID.Mod.GUID)) {
                return $"\"{this.Name}\"";
            } else {
                return $"\"{this.PBGID.Mod.GUID.Replace("-", "")}:{this.Name}\"";
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{this.BlueprintType}:{this.Name}";

        public override bool Equals(object obj) {
            if (obj is Blueprint bp && this != null) {
                return bp.BlueprintType == this.BlueprintType && bp.PBGID == this.PBGID;
            } else {
                return false;
            }
        }

        public override int GetHashCode() => this.PBGID.GetHashCode();

    }

}
