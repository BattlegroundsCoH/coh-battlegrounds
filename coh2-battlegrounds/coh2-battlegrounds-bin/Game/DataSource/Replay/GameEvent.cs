using System;
using System.Text;

namespace Battlegrounds.Game.DataSource.Replay {
    
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
            => (this.Type < (byte)GameEventType.EVENT_MAX) ? 
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
        /// The type the issued command will target (??? = ???, 16 = entity, 32 = squad, ??? = ???)
        /// </summary>
        public ushort TargetType { get; }

        /// <summary>
        /// The attached message to a <see cref="GameEvent"/> of type <see cref="GameEventType.PCMD_BroadcastMessage"/>.
        /// </summary>
        public string AttachedMessage { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="eventData"></param>
        public GameEvent(TimeSpan timeStamp, byte[] eventData) {

            // Set values to null
            this.FirstPosition = null;
            this.SecondPosition = null;
            this.AttachedMessage = null;

            // Add timestamp
            this.TimeStamp = timeStamp;

            // Read the event type (a simple byte)
            this.Type = eventData[0];

            // Read player ID (a simple byte)
            this.PlayerID = eventData[3];

            // The target type
            this.TargetType = BitConverter.ToUInt16(eventData[6..8]); // Most likely an enum
                                                                      // 16 = Entity
                                                                      // 32 = Squad
                                                                      // 64 = ???
                                                                      // ... = ...
                                                                      // ??? = ???

            // Read type-specific content
            if (this.Type < (byte)GameEventType.EVENT_MAX) {
                switch (this.EventType) {
                    case GameEventType.CMD_BuildSquad:
                    case GameEventType.CMD_Upgrade:
                    case GameEventType.SCMD_Upgrade:
                        this.UnitID = BitConverter.ToUInt16(eventData[8..10]);
                        this.BlueprintID = BitConverter.ToUInt32(eventData[13..17]);
                        break;
                    case GameEventType.SCMD_Move:
                        this.UnitID = BitConverter.ToUInt16(eventData[8..10]);
                        break;
                    // TODO: Other important cases here
                    // These load unit ID differently:
                    // SCMD_STOP
                    // SCMD_ATTACK
                    // SCMD_RETREAT
                    // SCMD_REINFORCEUNIT
                    // ...
                    case GameEventType.PCMD_BroadcastMessage:
                        int messageLength = (int)BitConverter.ToUInt32(eventData[20..24]);
                        this.AttachedMessage = Encoding.ASCII.GetString(eventData[24..(24 + messageLength)]);
                        break;
                    default: break;
                }
            }

        }

    }

}
