using System.Collections.Generic;
using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Gameplay {
    
    /// <summary>
    /// Representation of a Squad
    /// </summary>
    public class Squad {

        byte m_veterancyRank;
        byte m_veterancyProgress;

        private HashSet<Blueprint> m_upgrades;
        private HashSet<Blueprint> m_slotItems;

        /// <summary>
        /// The unique squad ID used to identify the <see cref="Squad"/>.
        /// </summary>
        public ushort SquadID { get; }

        /// <summary>
        /// The player who (currently) owns the <see cref="Squad"/>.
        /// </summary>
        public Player PlayerOwner { get; }

        /// <summary>
        /// The <see cref="Database.Blueprint"/> the <see cref="Squad"/> is a type of.
        /// </summary>
        public Blueprint Blueprint { get; }

        /// <summary>
        /// The achieved veterancy rank of a <see cref="Squad"/>
        /// </summary>
        public byte VeterancyRank => this.m_veterancyRank;

        /// <summary>
        /// The current veterancy progress of a <see cref="Squad"/>
        /// </summary>
        public byte VeterancyProgress => this.m_veterancyProgress;

        /// <summary>
        /// Create new <see cref="Squad"/> instance with a unique squad ID, a <see cref="Player"/> owner and a <see cref="Database.Blueprint"/>.
        /// </summary>
        /// <param name="squadID">The unique squad ID used to identify the squad</param>
        /// <param name="owner">The <see cref="Player"/> who owns the squad</param>
        /// <param name="squadBlueprint">The <see cref="Database.Blueprint"/> the squad is an instance of</param>
        public Squad(ushort squadID, Player owner, Blueprint squadBlueprint) {
            this.SquadID = squadID;
            this.PlayerOwner = owner;
            this.Blueprint = squadBlueprint;
            this.m_slotItems = new HashSet<Blueprint>();
            this.m_upgrades = new HashSet<Blueprint>();
        }

        /// <summary>
        /// Set the veterancy of the <see cref="Squad"/>. The rank and progress is not checked with the blueprint - any veterancy level can be achieved here.
        /// </summary>
        /// <param name="rank">The rank (or level) the squad has achieved.</param>
        /// <param name="progress">The current progress towards the next veterancy level</param>
        public void SetVeterancy(byte rank, byte progress = 0) {
            this.m_veterancyRank = rank;
            this.m_veterancyProgress = progress;
        }

        /// <summary>
        /// Add an upgrade to the squad
        /// </summary>
        /// <param name="upgradeBP">The upgrade blueprint to add</param>
        public void AddUpgrade(Blueprint upgradeBP) => this.m_upgrades.Add(upgradeBP);

        /// <summary>
        /// Add a slot item to the squad
        /// </summary>
        /// <param name="slotItemBP">The slot item blueprint to add</param>
        public void AddSlotItem(Blueprint slotItemBP) => this.m_slotItems.Add(slotItemBP);

    }

}
