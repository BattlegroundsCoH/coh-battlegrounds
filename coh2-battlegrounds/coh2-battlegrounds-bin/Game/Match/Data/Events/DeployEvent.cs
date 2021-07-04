using System;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

    /// <summary>
    /// <see cref="IMatchEvent"/> implementation for the event of squads being deployed.
    /// </summary>
    public class DeployEvent : IMatchEvent {
        
        public char Identifier => 'D';

        public uint Uid { get; }

        /// <summary>
        /// Get the <see cref="Player"/> who deployed the unit.
        /// </summary>
        public Player DeployingPlayer { get; }

        /// <summary>
        /// Get the squad ID of the unit that was deployed.
        /// </summary>
        public ushort SquadID { get; }

        /// <summary>
        /// Create a new <see cref="DeployEvent"/>.
        /// </summary>
        /// <param name="values">The string argument containing the ID.</param>
        /// <param name="player">The deploying player</param>
        /// <exception cref="FormatException"/>
        public DeployEvent(uint id, string[] values, Player player) {
            this.Uid = id;
            this.DeployingPlayer = player;
            if (ushort.TryParse(values[0], out ushort sid)) {
                this.SquadID = sid;
            } else {
                throw new FormatException();
            }
        }

    }

}
