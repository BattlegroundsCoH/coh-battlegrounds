using System;
using System.Collections.Generic;
using System.Text;

namespace coh2_battlegrounds_bin.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public class Player {

        public uint ID { get; }

        public string Name { get; }

        public string Profile { get; }

        public Faction Army { get; }

        public Player(uint id, string name, Faction faction, string aiprofile) {
            this.ID = id;
            this.Name = name;
            this.Army = faction;
            this.Profile = aiprofile;
        }

        public override string ToString() => $"{Name} ({Army.Name})";

    }

}
