using System.Collections.Generic;
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
        public void AddSquad(Squad squad) => this.m_activeSquads.Add(squad);

        /// <summary>
        /// 
        /// </summary>
        public void RemoveSquad(Squad squad) => this.m_activeSquads.Remove(squad);

        /// <summary>
        /// 
        /// </summary>
        public void AddEntity(Entity entity) => this.m_activeEntities.Add(entity);

        /// <summary>
        /// 
        /// </summary>
        public void RemoveEntity(Entity entity) => this.m_activeEntities.Remove(entity);

    }

}
