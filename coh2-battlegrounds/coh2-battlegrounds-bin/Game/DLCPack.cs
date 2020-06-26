namespace Battlegrounds.Game {
    
    /// <summary>
    /// Represents a DLC tied to Company of Heroes 2
    /// </summary>
    public sealed class DLCPack {

        /// <summary>
        /// The type of DLC a <see cref="DLCPack"/> may be of
        /// </summary>
        public enum DLCType {

            /// <summary>
            /// Faction that unlocks and army
            /// </summary>
            Faction,
            
            /// <summary>
            /// Vehicle Skin
            /// </summary>
            Skin,
            
            /// <summary>
            /// Ingame commander
            /// </summary>
            Commander,

        }

        /// <summary>
        /// The name of the DLC pack
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Unique app ID assigned to this pack
        /// </summary>
        public uint SteamAppID { get; }

        /// <summary>
        /// The type of DLC
        /// </summary>
        public DLCType Type { get; }

        private DLCPack(string name, uint steamid, DLCType dlctype) {
            this.Name = name;
            this.SteamAppID = steamid;
            this.Type = dlctype;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"DLC: {this.Name} (Steam App ID: {this.SteamAppID})";

        #region Static Region 

        private static DLCPack basegame;
        private static DLCPack wfa_aef;
        private static DLCPack wfa_okw;
        private static DLCPack ukf;

        /// <summary>
        /// The base game (with multiplayer access)
        /// </summary>
        public static DLCPack Base => basegame;

        /// <summary>
        /// The Western Front Armies (USA)
        /// </summary>
        public static DLCPack WesternFrontArmiesUSA => wfa_aef;

        /// <summary>
        /// The Western Front Armies (OKW)
        /// </summary>
        public static DLCPack WesternFrontArmiesOKW => wfa_okw;

        /// <summary>
        /// The British Forces
        /// </summary>
        public static DLCPack UKF => ukf;

        static DLCPack() {

            basegame = new DLCPack("Base Game", 231451, DLCType.Faction); // DLC ID

            wfa_aef = new DLCPack("Western Front Armies - US Forces", 39986, DLCType.Faction); // App ID

            wfa_okw = new DLCPack("Western Front Armies - Oberkommando West", 39988, DLCType.Faction); // App ID

            ukf = new DLCPack("British Forces", 76411, DLCType.Faction); // App ID

        }

        #endregion

    }

}
