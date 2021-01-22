using System;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

    /// <summary>
    /// <see cref="IMatchEvent"/> implementation for the event of a squad picking up an item.
    /// </summary>
    public class PickupEvent : IMatchEvent {

        public char Identifier => 'I';
        public uint Uid { get; }

        /// <summary>
        /// Get the player owning the squad that picked up the blueprint.
        /// </summary>
        public Player PickupPlayer { get; }

        /// <summary>
        /// Get the ID of the squad that picked up the blueprint.
        /// </summary>
        public ushort PickupSquadID { get; }

        /// <summary>
        /// Get the <see cref="Blueprint"/> of the item that was picked up.
        /// </summary>
        public Blueprint PickupItem { get; }

        /// <summary>
        /// Create a new <see cref="PickupEvent"/>.
        /// </summary>
        /// <param name="values">The string values for the event.</param>
        /// <param name="player">The player owning the unit picking up the item.</param>
        /// <exception cref="FormatException"/>
        public PickupEvent(uint id, string[] values, Player player) {

            // Set event ID
            this.Uid = id;

            // Set the pickup player
            this.PickupPlayer = player;

            // Get the squad ID
            if (ushort.TryParse(values[0], out ushort sid)) {
                this.PickupSquadID = sid;
            } else {
                throw new FormatException();
            }

            // Get the blueprint of the item that was picked up (may be null)
            this.PickupItem = BlueprintManager.FromBlueprintName(values[1], BlueprintType.IBP);

        }

    }

}
