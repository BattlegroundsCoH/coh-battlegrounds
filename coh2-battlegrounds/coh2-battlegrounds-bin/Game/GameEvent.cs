using System;
using System.Collections.Generic;
using System.Text;

namespace coh2_battlegrounds_bin.Game {
    
    /// <summary>
    /// 
    /// </summary>
    public class GameEvent {

        /// <summary>
        /// 
        /// </summary>
        public GamePosition? FirstPosition { get; }

        /// <summary>
        /// 
        /// </summary>
        public GamePosition? SecondPosition { get; }

        /// <summary>
        /// 
        /// </summary>
        public byte Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort PlayerID { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort ActionID { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort UnitID { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public GameEvent(byte[] eventData) {

            this.FirstPosition = null;
            this.SecondPosition = null;

            if (eventData.Length > 16) {

                this.Type = eventData[2];

                this.PlayerID = BitConverter.ToUInt16(eventData[4..6]);
                this.UnitID = BitConverter.ToUInt16(eventData[10..12]);
                this.ActionID = BitConverter.ToUInt16(eventData[14..16]);

                Console.WriteLine(this.PlayerID);

                if (eventData.Length > 29) { // positional

                    this.FirstPosition = new GamePosition(
                        BitConverter.ToSingle(eventData[18..22]), 
                        BitConverter.ToSingle(eventData[22..26]), 
                        BitConverter.ToSingle(eventData[26..30])
                        );

                    if (eventData.Length > 41) { // targetted

                        this.SecondPosition = new GamePosition(
                        BitConverter.ToSingle(eventData[30..34]),
                        BitConverter.ToSingle(eventData[34..38]),
                        BitConverter.ToSingle(eventData[38..42])
                        );

                    }

                }

            }

        }

    }

}
