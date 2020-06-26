using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Battlegrounds.Game.Database.json;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Steam;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// 
    /// </summary>
    public class Company : IJsonOjbect {

        private List<Squad> m_squads;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Faction Army { get; }

        /// <summary>
        /// 
        /// </summary>
        public SteamUser Owner { get; }

        /// <summary>
        /// The units of the company
        /// </summary>
        public ImmutableArray<Squad> Units => m_squads.ToImmutableArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <param name="army"></param>
        public Company(SteamUser user, string name, Faction army) {
            this.Name = name;
            this.Owner = user;
            this.Army = army;
            this.m_squads = new List<Squad>();
        }



    }

}
