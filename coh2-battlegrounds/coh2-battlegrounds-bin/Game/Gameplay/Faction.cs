using System;
using System.Collections.Generic;
using System.Text;

namespace coh2_battlegrounds_bin.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class Faction {

        /// <summary>
        /// 
        /// </summary>
        public byte UID { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAllied { get; }

        /// <summary>
        /// 
        /// </summary>
        public DLCPack RequiredDLC { get; }

        private Faction(byte id, string name, bool isAllied, DLCPack requiredDLC) {
            this.UID = id;
            this.Name = name;
            this.IsAllied = isAllied;
            this.RequiredDLC = requiredDLC;
        }

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
        /// 
        /// </summary>
        public static Faction Soviet => base_soviet;

        /// <summary>
        /// 
        /// </summary>
        public static Faction Wehrmacht => base_wehrmacht;

        /// <summary>
        /// 
        /// </summary>
        public static Faction America => wfa_aef;

        /// <summary>
        /// 
        /// </summary>
        public static Faction OberkommandoWest => wfa_okw;

        /// <summary>
        /// 
        /// </summary>
        public static Faction British => dlc_ukf;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        #endregion

    }

}
