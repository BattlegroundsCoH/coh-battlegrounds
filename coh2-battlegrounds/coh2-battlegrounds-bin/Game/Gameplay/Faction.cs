using System;
using System.Collections.Generic;

namespace Battlegrounds.Game.Gameplay {

    /// <summary>
    /// Represents a faction in Company of Heroes 2. This class cannot be inherited. This class cannot be instantiated.
    /// </summary>
    public sealed class Faction {

        /// <summary>
        /// The unique ID assigned to this <see cref="Faction"/>.
        /// </summary>
        public byte UID { get; }

        /// <summary>
        /// The SCAR name for this <see cref="Faction"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Is this <see cref="Faction"/> an allied faction.
        /// </summary>
        public bool IsAllied { get; }

        /// <summary>
        /// Is this <see cref="Faction"/> an axis faction.
        /// </summary>
        public bool IsAxis => !IsAllied;

        /// <summary>
        /// The required <see cref="DLCPack"/> to be able to play with this <see cref="Faction"/>.
        /// </summary>
        public DLCPack RequiredDLC { get; }

        private Faction(byte id, string name, bool isAllied, DLCPack requiredDLC) { // Private constructor, we can't have custom armies, doesn't make sense to allow them then.
            this.UID = id;
            this.Name = name;
            this.IsAllied = isAllied;
            this.RequiredDLC = requiredDLC;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => this.Name;

        #region Static Region

        public static implicit operator string(Faction fac) => fac.Name;

        public static explicit operator Faction(string name) => FromName(name);

        private static readonly Faction base_soviet = new Faction(0, "soviet", true, DLCPack.Base);
        private static readonly Faction base_wehrmacht = new Faction(1, "german", false, DLCPack.Base);

        private static readonly Faction wfa_aef = new Faction(2, "aef", true, DLCPack.WesternFrontArmiesUSA);
        private static readonly Faction wfa_okw = new Faction(3, "west_german", false, DLCPack.WesternFrontArmiesOKW);

        private static readonly Faction dlc_ukf = new Faction(4, "british", true, DLCPack.UKF);

        /// <summary>
        /// The Soviet faction.
        /// </summary>
        public static Faction Soviet => base_soviet;

        /// <summary>
        /// The Wehrmacht (Ostheer) faction.
        /// </summary>
        public static Faction Wehrmacht => base_wehrmacht;

        /// <summary>
        /// The US Forces faction.
        /// </summary>
        public static Faction America => wfa_aef;

        /// <summary>
        /// The Oberkommando West faction.
        /// </summary>
        public static Faction OberkommandoWest => wfa_okw;

        /// <summary>
        /// The United Kingdom faction.
        /// </summary>
        public static Faction British => dlc_ukf;

        /// <summary>
        /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
        /// </summary>
        /// <param name="name">The <see cref="string"/> faction name to find.</param>
        /// <returns>One of the five factions or null.</returns>
        public static Faction FromName(string name) {
            return (name.ToLower()) switch {
                "soviet" => base_soviet,
                "german" => base_wehrmacht,
                "usa" or "aef" => wfa_aef,
                "okw" or "west_german" => wfa_okw,
                "ukf" or "british" => dlc_ukf,
                _ => null,
            };
        }

        /// <summary>
        /// Returns the complementary faction of the given faction (eg. Wehrmacht - Soviet).
        /// </summary>
        /// <param name="faction">The faction to find the complement of.</param>
        /// <returns>The comlementary faction.</returns>
        /// <exception cref="ArgumentException"/>
        public static Faction GetComplementaryFaction(Faction faction) {
            if (faction == Soviet) {
                return Wehrmacht;
            } else if (faction == America) {
                return OberkommandoWest;
            } else if (faction == British) {
                return OberkommandoWest;
            } else if (faction == OberkommandoWest) {
                return America;
            } else if (faction == Wehrmacht) {
                return Soviet;
            } else {
                throw new ArgumentException("Unknown or custom faction - Not allowed!");
            }
        }

        /// <summary>
        /// Check if two factions are on the same team (Both are allied or both are axis)
        /// </summary>
        /// <param name="left">The first faction.</param>
        /// <param name="right">The second faction.</param>
        /// <returns><see langword="true"/> if both are on the same team. Otherwise <see langword="false"/>.</returns>
        public static bool AreSameTeam(Faction left, Faction right) => left.IsAllied == right.IsAllied || left.IsAxis == right.IsAxis;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonReference"></param>
        /// <returns></returns>
        public static object JsonDereference(string jsonReference) => FromName(jsonReference);

        public static List<Faction> Factions => new List<Faction> { Soviet, British, America, Wehrmacht, OberkommandoWest };

        #endregion

    }

}
