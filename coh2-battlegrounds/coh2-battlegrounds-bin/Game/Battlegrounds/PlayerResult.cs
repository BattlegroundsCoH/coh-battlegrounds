using System.Collections.Generic;
using System.Collections.Immutable;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Class the keeps track of <see cref="Gameplay.Player"/> data throughout a <see cref="GameMatch"/>.
    /// </summary>
    public class PlayerResult {

        private HashSet<Squad> m_activeSquads;
        private HashSet<Squad> m_lostSquads;
        private HashSet<Entity> m_activeEntities;

        /// <summary>
        /// Was this player on the winning team
        /// </summary>
        public bool IsOnWinningTeam { get; set; }

        /// <summary>
        /// The total amount of losses in squads (NOT squad members).
        /// </summary>
        public uint TotalLosses => (uint)m_lostSquads.Count;

        /// <summary>
        /// The <see cref="Player"/> the <see cref="PlayerResult"/> data is about.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// 
        /// </summary>
        public ImmutableHashSet<Squad> Losses => m_lostSquads.ToImmutableHashSet();

        /// <summary>
        /// 
        /// </summary>
        public ImmutableHashSet<Squad> Alive => m_activeSquads.ToImmutableHashSet();

        /// <summary>
        /// Keeps track of items captured by the player during the match.
        /// </summary>
        public List<Blueprint> CapturedItems { get; set; }

        /// <summary>
        /// Creates a new result container for the player.
        /// </summary>
        /// <param name="player">The player to keep all results about.</param>
        public PlayerResult(Player player) {
            this.Player = player;
            this.CapturedItems = new List<Blueprint>();
            this.m_activeEntities = new HashSet<Entity>();
            this.m_activeSquads = new HashSet<Squad>();
            this.m_lostSquads = new HashSet<Squad>();
        }

        /// <summary>
        /// Add a <see cref="Squad"/> to the active squad list for the player.
        /// </summary>
        /// <param name="squad">The new <see cref="Squad"/> to add to player squad set</param>
        public void AddSquad(Squad squad) => this.m_activeSquads.Add(squad);

        /// <summary>
        /// Remove a squad from the active squad list for the player and move it to the lost squads list.
        /// </summary>
        /// <param name="squad">The <see cref="Squad"/> to remove from the player squad list.</param>
        public void RemoveSquad(Squad squad) {
            this.m_activeSquads.Remove(squad);
            this.m_lostSquads.Add(squad);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(Entity entity) => this.m_activeEntities.Add(entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveEntity(Entity entity) => this.m_activeEntities.Remove(entity);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.Player} [{((this.IsOnWinningTeam)?"Won":"Lost")}]";

    }

}
