using System;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

    /// <summary>
    /// <see cref="IMatchEvent"/> implementation for the event of a squad being withdrawn from combat.
    /// </summary>
    public class RetreatEvent : IMatchEvent {

        public char Identifier => 'R';
        public uint Uid { get; }

        /// <summary>
        /// Get the player calling for withdrawal
        /// </summary>
        public Player WithdrawPlayer { get; }

        /// <summary>
        /// Get the ID of the withdrawing unit.
        /// </summary>
        public ushort WithdrawingUnitID { get; }

        /// <summary>
        /// Get the achieved veterancy of the withdrawing unit.
        /// </summary>
        public byte WithdrawingUnitVeterancyChange { get; }

        /// <summary>
        /// Get the achieved veterancy experience of the withdrawing unit.
        /// </summary>
        public float WithdrawingUnitVeterancyExperience { get; }

        /// <summary>
        /// Create a new <see cref="RetreatEvent"/>.
        /// </summary>
        /// <param name="values">The string arguments for the event.</param>
        /// <param name="player">The <see cref="Player"/> owning the retreating/withdrawing unit.</param>
        /// <exception cref="FormatException"/>
        public RetreatEvent(uint id, string[] values, Player player) {

            // Set event ID
            this.Uid = id;

            // Set player
            this.WithdrawPlayer = player;
            
            // Get unit ID
            if (ushort.TryParse(values[0], out ushort sid)) {
                this.WithdrawingUnitID = sid;
            } else {
                throw new FormatException();
            }
            
            // Get the veterancy change
            if (sbyte.TryParse(values[1], out sbyte change)) {
                this.WithdrawingUnitVeterancyChange = change >= 0 ? (byte)change : 0;
            } else {
                throw new FormatException();
            }

            // Get unit veterancy experience (We might only need to set this)
            if (float.TryParse(values[2], out float vet)) {
                this.WithdrawingUnitVeterancyExperience = vet;
            } else {
                throw new FormatException();
            }

        }

    }

}
