using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.json;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.Gameplay {
    
    /// <summary>
    /// Representation of a Squad
    /// </summary>
    public class Squad : IJsonObject, IChecksumItem {

        byte m_veterancyRank;
        float m_veterancyProgress;

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
        /// The <see cref="Blueprint"/> in a <see cref="SquadBlueprint"/> form.
        /// </summary>
        /// <exception cref="System.InvalidCastException"/>
        public SquadBlueprint SBP => this.Blueprint as SquadBlueprint;

        /// <summary>
        /// The achieved veterancy rank of a <see cref="Squad"/>
        /// </summary>
        public byte VeterancyRank => this.m_veterancyRank;

        /// <summary>
        /// The current veterancy progress of a <see cref="Squad"/>
        /// </summary>
        public float VeterancyProgress => this.m_veterancyProgress;

        /// <summary>
        /// The current upgrades applied to a <see cref="Squad"/>
        /// </summary>
        public ImmutableArray<Blueprint> Upgrades => m_upgrades.ToImmutableArray();

        /// <summary>
        /// The current slot items carried by the <see cref="Squad"/>
        /// </summary>
        public ImmutableArray<Blueprint> SlotItems => m_slotItems.ToImmutableArray();

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
        public void SetVeterancy(byte rank, float progress = 0.0f) {
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


        /// <summary>
        /// Calculate the actual cost of a <see cref="Squad"/>.
        /// </summary>
        /// <returns>The cost of the squad.</returns>
        public Cost GetCost() {

            Cost c = new Cost(SBP.Cost.Manpower, SBP.Cost.Munitions, SBP.Cost.Fuel, SBP.Cost.FieldTime);
            c = this.m_upgrades.Select(x => (x as UpgradeBlueprint).Cost).Aggregate(c, (a, b) => a + b);

            // TODO: More here

            return c;

        }

        /// <summary>
        /// Get the complete checksum in string format.
        /// </summary>
        /// <returns>The string representation of the checksum.</returns>
        public string GetChecksum() => throw new NotImplementedException();

    }

}
