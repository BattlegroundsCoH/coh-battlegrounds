using System;

namespace coh2_battlegrounds_bin.Game {
    
    /// <summary>
    /// Represents an event that occured in a <see cref="GameTick"/>.
    /// </summary>
    public class GameEvent {

        /// <summary>
        /// The first <see cref="GamePosition"/> argument given to the <see cref="GameEvent"/>.
        /// </summary>
        public GamePosition? FirstPosition { get; }

        /// <summary>
        /// The second <see cref="GamePosition"/> argument given to the <see cref="GameEvent"/>.
        /// </summary>
        public GamePosition? SecondPosition { get; }

        /// <summary>
        /// The byte representation of the <see cref="GameEvent"/> type
        /// </summary>
        public byte Type { get; }

        /// <summary>
        /// The <see cref="GameEventType"/> representation of the Type. Make sure the <see cref="GameEvent"/> is within range before using.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public GameEventType EventType 
            => (this.Type <= (byte)GameEventType.PCMD_COUNT) ? 
            ((GameEventType)this.Type) : 
            throw new ArgumentOutOfRangeException($"The event type {this.Type} is out of range and cannot be interpreted.");

        /// <summary>
        /// The byte-ID of the player who triggered or was affected by the <see cref="GameEvent"/>.
        /// </summary>
        public byte PlayerID { get; }

        /// <summary>
        /// The 16-bit ID representation of the affected unit
        /// </summary>
        public ushort UnitID { get; }

        /// <summary>
        /// The 32-bit ID of a blueprint ID that was given as argument to the <see cref="GameEvent"/>. Depending on <see cref="EventType"/>, this may be null.
        /// </summary>
        public uint? BlueprintID { get; }

        /// <summary>
        /// The calculated time in which the event occured
        /// </summary>
        public TimeSpan TimeStamp { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="eventData"></param>
        public GameEvent(TimeSpan timeStamp, byte[] eventData) {

            this.FirstPosition = null;
            this.SecondPosition = null;

            // Add timestamp
            this.TimeStamp = timeStamp;

            // Read the event type (a simple byte)
            this.Type = eventData[0];

            // Read player ID (a simple byte)
            this.PlayerID = eventData[3];

            // Read type-specific content
            if (this.Type <= (byte)GameEventType.PCMD_COUNT) {
                switch (this.EventType) {
                    case GameEventType.CMD_BuildSquad:
                    case GameEventType.CMD_Upgrade:
                    case GameEventType.SCMD_Upgrade:
                        this.BlueprintID = BitConverter.ToUInt32(eventData[13..17]);
                        break;
                        // TODO: Other important cases here
                    default: break;
                }

            }

        }

    }

}
