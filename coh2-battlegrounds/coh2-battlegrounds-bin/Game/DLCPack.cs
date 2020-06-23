using System;
using System.Collections.Generic;
using System.Text;

namespace coh2_battlegrounds_bin.Game {
    
    /// <summary>
    /// 
    /// </summary>
    public class DLCPack {
    
        private DLCPack() {

        }

        #region Static Region 

        private static DLCPack basegame;
        private static DLCPack wfa_aef;
        private static DLCPack wfa_okw;
        private static DLCPack wfa;
        private static DLCPack ukf;

        public static DLCPack Base => basegame;

        public static DLCPack WesternFrontArmies => wfa;

        public static DLCPack WesternFrontArmiesUSA => wfa_aef;

        public static DLCPack WesternFrontArmiesOKW => wfa_okw;

        public static DLCPack UKF => ukf;

        static DLCPack() {

        }

        #endregion

    }

}
