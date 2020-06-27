using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public class Player {

        /// <summary>
        /// 
        /// </summary>
        public uint ID { get; }

        /// <summary>
        /// 
        /// </summary>
        public uint TeamID { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Profile { get; }

        /// <summary>
        /// 
        /// </summary>
        public Faction Army { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAIPlayer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tID"></param>
        /// <param name="name"></param>
        /// <param name="faction"></param>
        /// <param name="aiprofile"></param>
        public Player(uint id, uint tID, string name, Faction faction, string aiprofile) {
            this.ID = id;
            this.TeamID = tID;
            this.Name = name;
            this.Army = faction;
            this.Profile = aiprofile;
            this.IsAIPlayer = false;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{Name} ({Army.Name})";

    }

}
