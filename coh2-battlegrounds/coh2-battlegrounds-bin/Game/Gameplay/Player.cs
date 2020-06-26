using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public class Player {

        public uint ID { get; }

        public uint TeamID { get; }

        public string Name { get; }

        public string Profile { get; }

        public Faction Army { get; }

        public bool IsAIPlayer { get; }

        public Player(uint id, uint tID, string name, Faction faction, string aiprofile) {
            this.ID = id;
            this.TeamID = tID;
            this.Name = name;
            this.Army = faction;
            this.Profile = aiprofile;
            this.IsAIPlayer = this.Profile != null;
        }

        public override string ToString() => $"{Name} ({Army.Name})";

    }

}
