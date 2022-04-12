using System;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

    /// <summary>
    /// <see cref="IMatchEvent"/> implementation for the event of squads being killed.
    /// </summary>
    public class KillEvent : IMatchEvent {

        public char Identifier => 'K';

        public uint Uid { get; }

        /// <summary>
        /// Get the player owning the lost unit.
        /// </summary>
        public Player UnitOwner { get; }

        /// <summary>
        /// Get the ID of the lost unit.
        /// </summary>
        public ushort UnitID { get; }

        /// <summary>
        /// Create a new <see cref="KillEvent"/>.
        /// </summary>
        /// <param name="values">The string argument containing the ID.</param>
        /// <param name="player">The player who is losing a unit.</param>
        /// <exception cref="FormatException"/>
        public KillEvent(uint id, string[] values, Player player) {
            this.Uid = id;
            this.UnitOwner = player;
            if (ushort.TryParse(values[0], out ushort sid)) {
                this.UnitID = sid;
            } else {
                throw new FormatException();
            }
        }

    }

}
