using System;
using System.Collections.Generic;
using System.Text;

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

        private static Faction base_soviet;
        private static Faction base_wehrmacht;

        private static Faction wfa_aef;
        private static Faction wfa_okw;

        private static Faction dlc_ukf;

        static Faction() {
            
            base_soviet = new Faction(0, "soviet", true, DLCPack.Base);

            base_wehrmacht = new Faction(1, "german", false, DLCPack.Base);

            wfa_aef = new Faction(2, "aef", true, DLCPack.WesternFrontArmiesUSA);

            wfa_okw = new Faction(3, "west_german", false, DLCPack.WesternFrontArmiesOKW);

            dlc_ukf = new Faction(4, "british", true, DLCPack.UKF);

        }

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
            switch (name.ToLower()) {
                case "soviet": return base_soviet;
                case "german": return base_wehrmacht;
                case "usa": 
                case "aef": return wfa_aef;
                case "okw":
                case "west_german": return wfa_okw;
                case "ukf":
                case "british": return dlc_ukf;
                default: return null;
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonReference"></param>
        /// <returns></returns>
        public static object JsonDereference(string jsonReference)  => FromName(jsonReference);

        #endregion

    }

}
