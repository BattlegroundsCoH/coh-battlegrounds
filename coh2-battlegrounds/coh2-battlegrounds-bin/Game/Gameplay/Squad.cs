using System;
using System.Collections.Generic;
using System.Text;
using coh2_battlegrounds_bin.Game.Database;

namespace coh2_battlegrounds_bin.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public class Squad {
    
        /// <summary>
        /// 
        /// </summary>
        public ushort SquadID { get; }

        /// <summary>
        /// 
        /// </summary>
        public Player PlayerOwner { get; }

        /// <summary>
        /// 
        /// </summary>
        public Blueprint Blueprint { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squadID"></param>
        /// <param name="owner"></param>
        /// <param name="squadBlueprint"></param>
        public Squad(ushort squadID, Player owner, Blueprint squadBlueprint) {
            this.SquadID = squadID;
            this.PlayerOwner = owner;
            this.Blueprint = squadBlueprint;
        }

    }

}
