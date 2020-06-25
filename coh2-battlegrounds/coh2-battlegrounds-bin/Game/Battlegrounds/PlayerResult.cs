using System;
using System.Collections.Generic;
using System.Text;
using coh2_battlegrounds_bin.Game.Gameplay;

namespace coh2_battlegrounds_bin.Game.Battlegrounds {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class PlayerResult {

        private HashSet<Squad> m_activeSquads;
        private HashSet<Entity> m_activeEntities;

        /// <summary>
        /// 
        /// </summary>
        public bool IsOnWinningTeam { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint TotalLosses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        public PlayerResult(Player player) {
            this.Player = player;
            this.m_activeEntities = new HashSet<Entity>();
            this.m_activeSquads = new HashSet<Squad>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddSquad() {

        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveSquad() {

        }

        /// <summary>
        /// 
        /// </summary>
        public void AddEntity() {

        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveEntity() {

        }

    }

}
